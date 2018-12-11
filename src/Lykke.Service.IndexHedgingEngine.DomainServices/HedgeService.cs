using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Infrastructure;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices
{
    public class HedgeService : IHedgeService
    {
        private readonly IIndexService _indexService;
        private readonly IIndexPriceService _indexPriceService;
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IAssetHedgeSettingsService _assetHedgeSettingsService;
        private readonly ITokenService _tokenService;
        private readonly IPositionService _positionService;
        private readonly IHedgeSettingsService _hedgeSettingsService;
        private readonly IInvestmentService _investmentService;
        private readonly IQuoteService _quoteService;
        private readonly IReadOnlyDictionary<string, IExchangeAdapter> _exchangeAdapters;
        private readonly ILog _log;

        public HedgeService(
            IIndexService indexService,
            IIndexPriceService indexPriceService,
            IIndexSettingsService indexSettingsService,
            IAssetHedgeSettingsService assetHedgeSettingsService,
            ITokenService tokenService,
            IPositionService positionService,
            IHedgeSettingsService hedgeSettingsService,
            IInvestmentService investmentService,
            IQuoteService quoteService,
            IExchangeAdapter[] exchangeAdapters,
            ILogFactory logFactory)
        {
            _indexService = indexService;
            _indexPriceService = indexPriceService;
            _indexSettingsService = indexSettingsService;
            _assetHedgeSettingsService = assetHedgeSettingsService;
            _tokenService = tokenService;
            _positionService = positionService;
            _hedgeSettingsService = hedgeSettingsService;
            _investmentService = investmentService;
            _quoteService = quoteService;
            _exchangeAdapters = exchangeAdapters.ToDictionary(exchange => exchange.Name, exchange => exchange);
            _log = logFactory.CreateLog(this);
        }

        public async Task ExecuteAsync()
        {
            string[] assets = await GetAssetsAsync();

            IReadOnlyCollection<Index> indices = _indexService.GetAll();

            IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

            IReadOnlyCollection<Token> tokens = await _tokenService.GetAllAsync();

            IReadOnlyCollection<Position> positions = await _positionService.GetAllAsync();

            IReadOnlyCollection<IndexPrice> indexPrices = await _indexPriceService.GetAllAsync();

            IReadOnlyDictionary<string, Quote> assetPrices = await GetAssetPricesAsync(assets);

            IReadOnlyCollection<AssetInvestment> assetInvestments = InvestmentCalculator.CalculateInvestments(assets,
                indicesSettings, tokens, indices, indexPrices, positions, assetPrices);

            IReadOnlyCollection<HedgeLimitOrder> hedgeLimitOrders = await CreateLimitOrdersAsync(assetInvestments);

            _log.InfoWithDetails("Hedge limit orders calculated", hedgeLimitOrders);

            _investmentService.Update(assetInvestments);

            await ApplyLimitOrdersAsync(assets, hedgeLimitOrders);
        }

        public async Task CreateLimitOrderAsync(string assetId, string exchange, LimitOrderType limitOrderType,
            decimal price, decimal volume, string userId)
        {
            AssetHedgeSettings assetHedgeSettings =
                await _assetHedgeSettingsService.GetByAssetIdAsync(assetId, exchange);

            if (assetHedgeSettings == null)
                throw new InvalidOperationException("Asset hedge settings not found");

            if (assetHedgeSettings.Mode != AssetHedgeMode.Manual)
                throw new InvalidOperationException("Asset hedge settings mode should be 'Manual'");

            if (!_exchangeAdapters.TryGetValue(assetHedgeSettings.Exchange, out IExchangeAdapter exchangeAdapter))
                throw new InvalidOperationException("There is no exchange provider");

            volume = Math.Round(volume, assetHedgeSettings.VolumeAccuracy);

            if (volume < assetHedgeSettings.MinVolume)
                throw new InvalidOperationException("The limit order volume is less than allowed min volume.");

            price = price.TruncateDecimalPlaces(assetHedgeSettings.PriceAccuracy,
                limitOrderType == LimitOrderType.Sell);

            HedgeLimitOrder hedgeLimitOrder = HedgeLimitOrder.Create(assetHedgeSettings.Exchange,
                assetHedgeSettings.AssetId, assetHedgeSettings.AssetPairId, limitOrderType, PriceType.Limit, price,
                volume);

            hedgeLimitOrder.Context = new {price, volume, userId}.ToJson();

            _log.InfoWithDetails("Manual hedge limit order created", new {hedgeLimitOrder, userId});

            await exchangeAdapter.ExecuteLimitOrderAsync(hedgeLimitOrder);
        }

        public async Task CancelLimitOrderAsync(string assetId, string exchange, string userId)
        {
            AssetHedgeSettings assetHedgeSettings =
                await _assetHedgeSettingsService.GetByAssetIdAsync(assetId, exchange);

            if (assetHedgeSettings == null)
                throw new InvalidOperationException("Asset hedge settings not found");

            if (!_exchangeAdapters.TryGetValue(assetHedgeSettings.Exchange, out IExchangeAdapter exchangeAdapter))
                throw new InvalidOperationException("There is no exchange provider");

            _log.InfoWithDetails("Hedge limit order cancelled by user", new {assetId, exchange, userId});

            await exchangeAdapter.CancelLimitOrderAsync(assetId);
        }

        public async Task ClosePositionAsync(string assetId, string exchange, string userId)
        {
            Position position = await _positionService.GetByAssetIdAsync(assetId, exchange);

            if (position == null)
                throw new InvalidOperationException("Position not found");

            AssetHedgeSettings assetHedgeSettings =
                await _assetHedgeSettingsService.GetByAssetIdAsync(assetId, exchange);

            if (assetHedgeSettings == null)
                throw new InvalidOperationException("Asset hedge settings not found");

            if (assetHedgeSettings.Mode == AssetHedgeMode.Auto)
                throw new InvalidOperationException("Can not close position while asset hedge settings mode is 'Auto'");

            if (!_exchangeAdapters.TryGetValue(assetHedgeSettings.Exchange, out IExchangeAdapter exchangeAdapter))
                throw new InvalidOperationException("There is no exchange provider");

            Quote quote = _quoteService.GetByAssetPairId(assetHedgeSettings.Exchange,
                assetHedgeSettings.AssetPairId);

            if (quote == null)
                throw new InvalidOperationException("No quote");

            decimal volume = Math.Round(position.Volume, assetHedgeSettings.VolumeAccuracy);

            if (Math.Abs(volume) < assetHedgeSettings.MinVolume)
                throw new InvalidOperationException("The limit order volume is less than allowed min volume.");

            LimitOrderType limitOrderType;
            decimal price;

            if (volume > 0)
            {
                limitOrderType = LimitOrderType.Sell;
                price = quote.Ask.TruncateDecimalPlaces(assetHedgeSettings.PriceAccuracy, true);
            }
            else
            {
                limitOrderType = LimitOrderType.Buy;
                price = quote.Bid.TruncateDecimalPlaces(assetHedgeSettings.PriceAccuracy);
            }

            HedgeLimitOrder hedgeLimitOrder = HedgeLimitOrder.Create(assetHedgeSettings.Exchange,
                assetHedgeSettings.AssetId, assetHedgeSettings.AssetPairId, limitOrderType, PriceType.Limit, price,
                volume);

            hedgeLimitOrder.Context = new {userId}.ToJson();

            _log.InfoWithDetails("Manual hedge limit order created to closed position", new {hedgeLimitOrder, userId});

            await exchangeAdapter.ExecuteLimitOrderAsync(hedgeLimitOrder);
        }

        private async Task<IReadOnlyCollection<HedgeLimitOrder>> CreateLimitOrdersAsync(
            IReadOnlyCollection<AssetInvestment> assetInvestments)
        {
            HedgeSettings hedgeSettings = await _hedgeSettingsService.GetAsync();

            var hedgeLimitOrders = new List<HedgeLimitOrder>();

            foreach (AssetInvestment assetInvestment in assetInvestments)
            {
                if (assetInvestment.IsDisabled)
                    continue;

                if (Math.Abs(assetInvestment.RemainingAmount) <= hedgeSettings.ThresholdDown)
                    continue;

                AssetHedgeSettings assetHedgeSettings =
                    await _assetHedgeSettingsService.EnsureAsync(assetInvestment.AssetId);

                if (assetHedgeSettings.Mode != AssetHedgeMode.Auto && assetHedgeSettings.Mode != AssetHedgeMode.Idle)
                    continue;

                if (assetInvestment.Quote == null)
                {
                    _log.WarningWithDetails("No quote to create hedge limit order",
                        new {assetHedgeSettings.Exchange, assetHedgeSettings.AssetPairId, assetInvestment});

                    // TODO: send health issue

                    continue;
                }

                decimal volume = Math.Round(Math.Abs(assetInvestment.RemainingAmount / assetInvestment.Quote.Mid),
                    assetHedgeSettings.VolumeAccuracy);

                LimitOrderType limitOrderType = assetInvestment.RemainingAmount > 0
                    ? LimitOrderType.Sell
                    : LimitOrderType.Buy;

                PriceType priceType;

                decimal price = InvestmentCalculator.CalculateHedgeLimitOrderPrice(assetInvestment.Quote,
                        assetInvestment.RemainingAmount, hedgeSettings, out priceType)
                    .TruncateDecimalPlaces(assetHedgeSettings.PriceAccuracy, limitOrderType == LimitOrderType.Sell);

                HedgeLimitOrder hedgeLimitOrder = HedgeLimitOrder.Create(assetHedgeSettings.Exchange,
                    assetHedgeSettings.AssetId, assetHedgeSettings.AssetPairId, limitOrderType, priceType, price,
                    volume);

                hedgeLimitOrder.Context = assetInvestment.ToJson();

                hedgeLimitOrders.Add(hedgeLimitOrder);
            }

            return hedgeLimitOrders;
        }

        private async Task ApplyLimitOrdersAsync(IEnumerable<string> assets,
            IReadOnlyCollection<HedgeLimitOrder> hedgeLimitOrders)
        {
            foreach (string assetId in assets)
            {
                AssetHedgeSettings assetHedgeSettings = await _assetHedgeSettingsService.GetByAssetIdAsync(assetId);

                HedgeLimitOrder hedgeLimitOrder = hedgeLimitOrders.SingleOrDefault(o => o.AssetId == assetId);

                if (_exchangeAdapters.TryGetValue(assetHedgeSettings.Exchange, out IExchangeAdapter exchangeAdapter))
                {
                    if (hedgeLimitOrder == null)
                    {
                        if (assetHedgeSettings.Mode != AssetHedgeMode.Manual)
                            await exchangeAdapter.CancelLimitOrderAsync(assetId);
                    }
                    else
                    {
                        if (assetHedgeSettings.Mode == AssetHedgeMode.Auto)
                            await exchangeAdapter.ExecuteLimitOrderAsync(hedgeLimitOrder);
                    }
                }
                else
                {
                    if (hedgeLimitOrder != null)
                    {
                        _log.WarningWithDetails("There is no exchange provider",
                            new {assetHedgeSettings, hedgeLimitOrder});
                        // TODO: send health issue
                    }
                }
            }
        }

        private async Task<string[]> GetAssetsAsync()
        {
            var assets = new string [0];

            IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

            foreach (IndexSettings indexSettings in indicesSettings)
            {
                Index index = _indexService.Get(indexSettings.Name);

                if (index?.Weights != null)
                    assets = assets.Union(index.Weights.Select(o => o.AssetId)).ToArray();
            }

            IReadOnlyCollection<Position> positions = await _positionService.GetAllAsync();

            assets = assets.Union(positions.Select(o => o.AssetId)).ToArray();

            return assets;
        }

        private async Task<IReadOnlyDictionary<string, Quote>> GetAssetPricesAsync(IEnumerable<string> assets)
        {
            var assetPrices = new Dictionary<string, Quote>();

            foreach (string assetId in assets)
            {
                AssetHedgeSettings assetHedgeSettings = await _assetHedgeSettingsService.EnsureAsync(assetId);

                Quote quote = _quoteService.GetByAssetPairId(assetHedgeSettings.Exchange,
                    assetHedgeSettings.AssetPairId);

                if (quote != null)
                    assetPrices[assetId] = quote;
            }

            return assetPrices;
        }
    }
}
