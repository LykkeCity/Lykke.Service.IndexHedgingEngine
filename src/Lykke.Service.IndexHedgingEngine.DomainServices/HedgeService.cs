using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Infrastructure;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;
using Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices
{
    public class HedgeService : IHedgeService
    {
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

        public async Task UpdateLimitOrdersAsync()
        {
            IReadOnlyCollection<string> assets = await GetAssetsAsync();

            await CancelLimitOrdersAsync(assets);

            if (!await ValidateIndexPricesAsync())
                return;

            IReadOnlyCollection<Token> tokens = await _tokenService.GetAllAsync();

            IReadOnlyCollection<Position> positions = await GetCurrentPositionsAsync();

            IReadOnlyCollection<IndexPrice> indexPrices = await _indexPriceService.GetAllAsync();

            IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

            IReadOnlyDictionary<string, Quote> assetPrices = await GetAssetPricesAsync(assets);

            IReadOnlyCollection<AssetInvestment> assetInvestments =
                InvestmentCalculator.Calculate(assets, indicesSettings, tokens, indexPrices, positions, assetPrices);

            _log.InfoWithDetails("Investments calculated", assetInvestments);

            IReadOnlyCollection<HedgeLimitOrder> hedgeLimitOrders = await CreateLimitOrdersAsync(assetInvestments);

            await ValidateHedgeLimitOrdersAsync(hedgeLimitOrders);

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

            LimitOrderType limitOrderType = position.Volume > 0
                ? LimitOrderType.Sell
                : LimitOrderType.Buy;

            decimal price = limitOrderType == LimitOrderType.Sell
                ? quote.Ask
                : quote.Bid;

            decimal volume = Math.Abs(position.Volume);

            HedgeLimitOrder hedgeLimitOrder = HedgeLimitOrder.Create(assetHedgeSettings.Exchange,
                assetHedgeSettings.AssetId, assetHedgeSettings.AssetPairId, limitOrderType, PriceType.Limit, price,
                volume);

            hedgeLimitOrder.Context = new {userId}.ToJson();

            _log.InfoWithDetails("Manual hedge limit order created to closed position", new {hedgeLimitOrder, userId});

            await exchangeAdapter.ExecuteLimitOrderAsync(hedgeLimitOrder);
        }

        private async Task<IReadOnlyCollection<HedgeLimitOrder>> CreateLimitOrdersAsync(
            IEnumerable<AssetInvestment> assetInvestments)
        {
            HedgeSettings hedgeSettings = await _hedgeSettingsService.GetAsync();

            var hedgeLimitOrders = new List<HedgeLimitOrder>();

            foreach (AssetInvestment assetInvestment in assetInvestments)
            {
                AssetHedgeSettings assetHedgeSettings =
                    await _assetHedgeSettingsService.EnsureAsync(assetInvestment.AssetId);

                if (!CanCreateHedgeLimitOrder(assetInvestment, assetHedgeSettings, hedgeSettings))
                    continue;

                LimitOrderType limitOrderType = assetInvestment.RemainingAmount > 0
                    ? LimitOrderType.Sell
                    : LimitOrderType.Buy;

                LimitOrderPrice limitOrderPrice = LimitOrderPriceCalculator.Calculate(assetInvestment.Quote,
                    Math.Abs(assetInvestment.RemainingAmount), limitOrderType,
                    assetHedgeSettings.ThresholdUp ?? hedgeSettings.ThresholdUp, hedgeSettings.MarketOrderMarkup);

                decimal price = limitOrderPrice.Price;

                decimal volume = Math.Abs(assetInvestment.RemainingAmount / assetInvestment.Quote.Mid);

                HedgeLimitOrder hedgeLimitOrder = HedgeLimitOrder.Create(assetHedgeSettings.Exchange,
                    assetHedgeSettings.AssetId, assetHedgeSettings.AssetPairId, limitOrderType, limitOrderPrice.Type,
                    price, volume);

                hedgeLimitOrder.Context = assetInvestment.ToJson();

                hedgeLimitOrders.Add(hedgeLimitOrder);
            }

            return hedgeLimitOrders;
        }

        private async Task CancelLimitOrdersAsync(IEnumerable<string> assets)
        {
            foreach (string assetId in assets)
            {
                AssetHedgeSettings assetHedgeSettings = await _assetHedgeSettingsService.EnsureAsync(assetId);

                if (_exchangeAdapters.TryGetValue(assetHedgeSettings.Exchange, out IExchangeAdapter exchangeAdapter))
                {
                    if (assetHedgeSettings.Mode != AssetHedgeMode.Manual)
                        await exchangeAdapter.CancelLimitOrderAsync(assetId);
                }
            }
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
                    if (hedgeLimitOrder == null || hedgeLimitOrder.Error != LimitOrderError.None)
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

        private async Task<bool> ValidateIndexPricesAsync()
        {
            bool valid = true;

            IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

            foreach (IndexSettings indexSettings in indicesSettings)
            {
                IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(indexSettings.Name);

                if (indexPrice == null)
                {
                    _log.WarningWithDetails("Index price not found", indexSettings.Name);
                    valid = false;
                }
                else if (!indexPrice.ValidateValue())
                {
                    _log.WarningWithDetails("Invalid index price", indexPrice);
                    valid = false;
                }
                else if (!indexPrice.ValidateWeights())
                {
                    _log.WarningWithDetails("Invalid index price weights", indexPrice);
                    valid = false;
                }
            }

            return valid;
        }

        private async Task ValidateHedgeLimitOrdersAsync(IEnumerable<HedgeLimitOrder> hedgeLimitOrders)
        {
            foreach (HedgeLimitOrder hedgeLimitOrder in hedgeLimitOrders)
            {
                if (hedgeLimitOrder.Error != LimitOrderError.None)
                    continue;

                AssetHedgeSettings assetHedgeSettings =
                    await _assetHedgeSettingsService.GetByAssetIdAsync(hedgeLimitOrder.AssetId);

                if (!string.IsNullOrEmpty(assetHedgeSettings.ReferenceExchange) &&
                    assetHedgeSettings.ReferenceDelta.HasValue)
                {
                    Quote quote = _quoteService.GetByAssetPairId(assetHedgeSettings.ReferenceExchange,
                        assetHedgeSettings.AssetPairId);

                    if (quote == null)
                    {
                        hedgeLimitOrder.Error = LimitOrderError.Unknown;
                        hedgeLimitOrder.ErrorMessage = "No reference quote";

                        continue;
                    }

                    if (Math.Abs(hedgeLimitOrder.Price - quote.Mid) / quote.Mid >= assetHedgeSettings.ReferenceDelta)
                    {
                        hedgeLimitOrder.Error = LimitOrderError.Unknown;
                        hedgeLimitOrder.ErrorMessage = "Large price deviation";
                    }
                }
            }
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

        private async Task<IReadOnlyCollection<Position>> GetCurrentPositionsAsync()
        {
            IReadOnlyCollection<Position> positions = await _positionService.GetAllAsync();

            var currentPositions = new List<Position>();

            foreach (IGrouping<string, Position> groupByAsset in positions.GroupBy(o => o.AssetId))
            {
                AssetHedgeSettings assetHedgeSettings =
                    await _assetHedgeSettingsService.GetByAssetIdAsync(groupByAsset.Key);

                if (assetHedgeSettings == null)
                    continue;

                Position currentPosition = groupByAsset
                    .SingleOrDefault(position => position.Exchange == assetHedgeSettings.Exchange);

                if (currentPosition != null)
                    currentPositions.Add(currentPosition);
            }

            return currentPositions;
        }

        private async Task<IReadOnlyCollection<string>> GetAssetsAsync()
        {
            IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

            IReadOnlyCollection<Position> positions = await _positionService.GetAllAsync();

            List<string> assets = positions.Select(o => o.AssetId).ToList();

            foreach (IndexSettings indexSettings in indicesSettings)
            {
                IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(indexSettings.Name);

                if (indexPrice != null)
                    assets.AddRange(indexPrice.Weights.Select(o => o.AssetId));
            }

            return assets.Distinct().ToList();
        }

        private static bool CanCreateHedgeLimitOrder(AssetInvestment assetInvestment,
            AssetHedgeSettings assetHedgeSettings, HedgeSettings hedgeSettings)
        {
            if (assetInvestment.IsDisabled)
                return false;

            decimal absoluteRemainingAmount = Math.Abs(assetInvestment.RemainingAmount);

            if (absoluteRemainingAmount <= 0)
                return false;

            decimal thresholdCritical = assetHedgeSettings.ThresholdCritical ?? hedgeSettings.ThresholdCritical;

            if (0 < thresholdCritical && thresholdCritical <= absoluteRemainingAmount)
                return false;

            decimal thresholdDown = assetHedgeSettings.ThresholdDown ?? hedgeSettings.ThresholdDown;

            if (assetHedgeSettings.Exchange != ExchangeNames.Virtual && absoluteRemainingAmount < thresholdDown)
                return false;

            if (assetHedgeSettings.Mode != AssetHedgeMode.Auto && assetHedgeSettings.Mode != AssetHedgeMode.Idle)
                return false;

            if (assetInvestment.Quote == null)
                return false;

            return true;
        }
    }
}
