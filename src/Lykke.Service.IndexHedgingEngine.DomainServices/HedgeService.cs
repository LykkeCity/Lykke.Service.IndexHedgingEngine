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
        private readonly IHedgeLimitOrderService _hedgeLimitOrderService;
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
            IHedgeLimitOrderService hedgeLimitOrderService,
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
            _hedgeLimitOrderService = hedgeLimitOrderService;
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
            
            await _hedgeLimitOrderService.UpdateAsync(hedgeLimitOrders);
            
            await ApplyLimitOrdersAsync(assets, hedgeLimitOrders);
        }

        private async Task<IReadOnlyCollection<HedgeLimitOrder>> CreateLimitOrdersAsync(
            IReadOnlyCollection<AssetInvestment> assetInvestments)
        {
            HedgeSettings hedgeSettings = await _hedgeSettingsService.GetAsync();

            var hedgeLimitOrders = new List<HedgeLimitOrder>();

            foreach (AssetInvestment assetInvestment in assetInvestments)
            {
                AssetHedgeSettings assetHedgeSettings =
                    await _assetHedgeSettingsService.EnsureAsync(assetInvestment.AssetId);

                if (!assetHedgeSettings.Approved)
                {
                    _log.WarningWithDetails("Asset hedge settings not approved", new {assetInvestment, hedgeSettings});

                    // TODO: send health issue

                    continue;
                }

                if (!assetHedgeSettings.Enabled)
                {
                    _log.InfoWithDetails("Asset hedging disabled", new {assetInvestment, hedgeSettings});

                    continue;
                }
                
                if (assetInvestment.Quote == null)
                {
                    _log.WarningWithDetails("No quote to create hedge limit order",
                        new {assetHedgeSettings.Exchange, assetHedgeSettings.AssetPairId, assetInvestment});

                    // TODO: send health issue

                    continue;
                }

                if (Math.Abs(assetInvestment.RemainingAmount) <= hedgeSettings.ThresholdDown)
                {
                    _log.InfoWithDetails("The asset investments is less than threshold down",
                        new {assetInvestment, hedgeSettings});

                    continue;
                }

                decimal volume = Math.Round(Math.Abs(assetInvestment.RemainingAmount / assetInvestment.Quote.Mid),
                    assetHedgeSettings.VolumeAccuracy);

                LimitOrderType limitOrderType = assetInvestment.RemainingAmount > 0
                    ? LimitOrderType.Sell
                    : LimitOrderType.Buy;

                decimal price = InvestmentCalculator.CalculateHedgeLimitOrderPrice(assetInvestment.Quote,
                        assetInvestment.RemainingAmount, hedgeSettings)
                    .TruncateDecimalPlaces(assetHedgeSettings.PriceAccuracy, limitOrderType == LimitOrderType.Sell);

                HedgeLimitOrder hedgeLimitOrder = HedgeLimitOrder.Create(assetHedgeSettings.Exchange,
                    assetHedgeSettings.AssetId, assetHedgeSettings.AssetPairId, limitOrderType, price, volume);

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

                if (_exchangeAdapters.TryGetValue(assetHedgeSettings.Exchange, out IExchangeAdapter exchange))
                {
                    if (hedgeLimitOrder == null)
                        await exchange.CancelLimitOrderAsync(assetId);
                    else
                        await exchange.ExecuteLimitOrderAsync(hedgeLimitOrder);
                }
                else if (hedgeLimitOrder != null)
                {
                    _log.WarningWithDetails("There is no exchange provider", new {assetHedgeSettings, hedgeLimitOrder});

                    // TODO: send health issue
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

                if (quote == null)
                {
                    _log.WarningWithDetails("No quote for asset", new
                    {
                        assetHedgeSettings.Exchange,
                        assetHedgeSettings.AssetPairId
                    });

                    continue;
                }

                assetPrices[assetId] = quote;
            }

            return assetPrices;
        }
    }
}
