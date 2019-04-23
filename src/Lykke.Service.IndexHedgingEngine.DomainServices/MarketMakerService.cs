using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;
using Lykke.Service.IndexHedgingEngine.DomainServices.Utils;

namespace Lykke.Service.IndexHedgingEngine.DomainServices
{
    [UsedImplicitly]
    public class MarketMakerService : IMarketMakerService
    {
        private const string Exchange = ExchangeNames.Lykke;

        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IIndexPriceService _indexPriceService;
        private readonly IBalanceService _balanceService;
        private readonly ISettingsService _settingsService;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly ILimitOrderService _limitOrderService;
        private readonly IInstrumentService _instrumentService;
        private readonly IQuoteService _quoteService;
        private readonly ICrossIndexSettingsService _crossIndexSettingsService;
        private readonly TraceWriter _traceWriter;
        private readonly ILog _log;

        public MarketMakerService(
            IIndexSettingsService indexSettingsService,
            IIndexPriceService indexPriceService,
            IBalanceService balanceService,
            ISettingsService settingsService,
            ILykkeExchangeService lykkeExchangeService,
            ILimitOrderService limitOrderService,
            IInstrumentService instrumentService,
            IQuoteService quoteService,
            ICrossIndexSettingsService crossIndexSettingsService,
            TraceWriter traceWriter,
            ILogFactory logFactory)
        {
            _indexSettingsService = indexSettingsService;
            _indexPriceService = indexPriceService;
            _balanceService = balanceService;
            _settingsService = settingsService;
            _lykkeExchangeService = lykkeExchangeService;
            _limitOrderService = limitOrderService;
            _instrumentService = instrumentService;
            _quoteService = quoteService;
            _crossIndexSettingsService = crossIndexSettingsService;
            _traceWriter = traceWriter;
            _log = logFactory.CreateLog(this);
        }

        public async Task UpdateLimitOrdersAsync(string indexName)
        {
            IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(indexName);

            if (indexPrice == null)
                throw new InvalidOperationException("Index price not found");

            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(indexName);

            if (indexSettings == null)
                throw new InvalidOperationException("Index settings not found");

            // original index limit orders
            await UpdateLimitOrdersAsync(indexPrice.Price, indexSettings);

            // cross index limit orders
            await UpdateCrossLimitOrdersAsync(indexPrice.Price, indexSettings);
        }

        public async Task CancelLimitOrdersAsync(string indexName)
        {
            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(indexName);

            if (indexSettings == null)
                throw new InvalidOperationException("Index settings not found");

            // cancel original index limit orders
            await _lykkeExchangeService.CancelAsync(indexSettings.AssetPairId);

            // cancel cross index limit orders
            await CancelCrossLimitOrdersAsync(indexSettings);

            _log.InfoWithDetails("Limit orders canceled", new { IndexName = indexName, indexSettings.AssetPairId });
        }

        private async Task UpdateLimitOrdersAsync(decimal indexPrice, IndexSettings indexSettings)
        {
            AssetPairSettings indexAssetPairSettings =
                await _instrumentService.GetAssetPairAsync(indexSettings.AssetPairId, Exchange);

            if (indexAssetPairSettings == null)
                throw new InvalidOperationException("Asset pair settings not found");

            AssetSettings indexBaseAssetSettings =
                await _instrumentService.GetAssetAsync(indexAssetPairSettings.BaseAsset, ExchangeNames.Lykke);

            if (indexBaseAssetSettings == null)
                throw new InvalidOperationException("Base asset settings not found");

            AssetSettings indexQuoteAssetSettings =
                await _instrumentService.GetAssetAsync(indexAssetPairSettings.QuoteAsset, ExchangeNames.Lykke);

            if (indexQuoteAssetSettings == null)
                throw new InvalidOperationException("Quote asset settings not found");

            decimal sellPrice = (indexPrice + indexSettings.SellMarkup)
                .TruncateDecimalPlaces(indexAssetPairSettings.PriceAccuracy, true);

            decimal buyPrice = indexPrice.TruncateDecimalPlaces(indexAssetPairSettings.PriceAccuracy);

            IReadOnlyCollection<LimitOrder> limitOrders =
                CreateLimitOrders(indexSettings, indexAssetPairSettings, sellPrice, buyPrice);

            ValidateBalance(limitOrders, indexBaseAssetSettings, indexQuoteAssetSettings);

            ValidateMinVolume(limitOrders, indexAssetPairSettings.MinVolume);

            LimitOrder[] allowedLimitOrders = limitOrders
                .Where(o => o.Error == LimitOrderError.None)
                .ToArray();

            _log.InfoWithDetails("Limit orders created", limitOrders);

            _limitOrderService.Update(indexSettings.AssetPairId, limitOrders);

            await _lykkeExchangeService.ApplyAsync(indexSettings.AssetPairId, allowedLimitOrders);

            _traceWriter.LimitOrders(indexSettings.AssetPairId, limitOrders);
        }

        private async Task UpdateCrossLimitOrdersAsync(decimal indexPrice, IndexSettings indexSettings)
        {
            IReadOnlyList<CrossIndexSettings> crossIndices =
                await _crossIndexSettingsService.FindByIndexAssetPairAsync(indexSettings.AssetPairId);

            foreach (CrossIndexSettings crossIndex in crossIndices)
            {
                IndexSettings newIndexSettings = await GetNewIndexSettingsForCrossIndex(indexSettings, crossIndex);

                decimal crossRate = GetCrossRate(crossIndex);

                decimal crossIndexPrice = indexPrice * crossRate;

                await UpdateLimitOrdersAsync(crossIndexPrice, newIndexSettings);
            }
        }

        private IReadOnlyCollection<LimitOrder> CreateLimitOrders(IndexSettings indexSettings,
            AssetPairSettings assetPairSettings, decimal sellPrice, decimal buyPrice)
        {
            var limitOrders = new List<LimitOrder>();

            string walletId = _settingsService.GetWalletId();

            decimal sellVolume = Math.Round(indexSettings.SellVolume / indexSettings.SellLimitOrdersCount,
                assetPairSettings.VolumeAccuracy);

            if (sellVolume >= assetPairSettings.MinVolume)
            {
                for (int i = 0; i < indexSettings.SellLimitOrdersCount; i++)
                    limitOrders.Add(LimitOrder.CreateSell(walletId, sellPrice, sellVolume));
            }
            else
            {
                limitOrders.Add(LimitOrder.CreateSell(walletId, sellPrice,
                    Math.Round(indexSettings.SellVolume, assetPairSettings.VolumeAccuracy)));
            }

            decimal buyVolume = Math.Round(indexSettings.BuyVolume / indexSettings.BuyLimitOrdersCount,
                assetPairSettings.VolumeAccuracy);

            if (buyVolume >= assetPairSettings.MinVolume)
            {
                for (int i = 0; i < indexSettings.BuyLimitOrdersCount; i++)
                    limitOrders.Add(LimitOrder.CreateBuy(walletId, buyPrice, buyVolume));
            }
            else
            {
                limitOrders.Add(LimitOrder.CreateBuy(walletId, buyPrice,
                    Math.Round(indexSettings.BuyVolume, assetPairSettings.VolumeAccuracy)));
            }

            return limitOrders;
        }

        private void ValidateBalance(IReadOnlyCollection<LimitOrder> limitOrders, AssetSettings baseAssetSettings,
            AssetSettings quoteAssetSettings)
        {
            List<LimitOrder> sellLimitOrders = limitOrders
                .Where(o => o.Error == LimitOrderError.None)
                .Where(o => o.Type == LimitOrderType.Sell)
                .OrderBy(o => o.Price)
                .ToList();

            List<LimitOrder> buyLimitOrders = limitOrders
                .Where(o => o.Error == LimitOrderError.None)
                .Where(o => o.Type == LimitOrderType.Buy)
                .OrderByDescending(o => o.Price)
                .ToList();

            if (sellLimitOrders.Any())
            {
                decimal balance = _balanceService.GetByAssetId(ExchangeNames.Lykke, baseAssetSettings.AssetId).Amount;

                foreach (LimitOrder limitOrder in sellLimitOrders)
                {
                    decimal amount = limitOrder.Volume.TruncateDecimalPlaces(baseAssetSettings.Accuracy, true);

                    if (balance - amount < 0)
                    {
                        decimal volume = balance.TruncateDecimalPlaces(baseAssetSettings.Accuracy);

                        limitOrder.UpdateVolume(volume);
                    }

                    balance = Math.Max(balance - amount, 0);
                }
            }

            if (buyLimitOrders.Any())
            {
                decimal balance = _balanceService.GetByAssetId(ExchangeNames.Lykke, quoteAssetSettings.AssetId).Amount;

                foreach (LimitOrder limitOrder in buyLimitOrders)
                {
                    decimal amount = (limitOrder.Price * limitOrder.Volume)
                        .TruncateDecimalPlaces(quoteAssetSettings.Accuracy, true);

                    if (balance - amount < 0)
                    {
                        decimal volume = (balance / limitOrder.Price).TruncateDecimalPlaces(baseAssetSettings.Accuracy);

                        limitOrder.UpdateVolume(volume);
                    }

                    balance = Math.Max(balance - amount, 0);
                }
            }
        }

        private static void ValidateMinVolume(IEnumerable<LimitOrder> limitOrders, decimal minVolume)
        {
            foreach (LimitOrder limitOrder in limitOrders.Where(o => o.Error == LimitOrderError.None))
            {
                if (limitOrder.Volume < minVolume || limitOrder.Volume <= 0)
                    limitOrder.Error = LimitOrderError.TooSmallVolume;
            }
        }

        private static IndexSettings GetNewIndexSettingsForCrossIndex(IndexSettings indexSettings,
            AssetPairSettings crossIndexAssetPairSettings, decimal crossRate)
        {
            IndexSettings result = new IndexSettings
            {
                Name = indexSettings.Name,
                AssetId = indexSettings.AssetId,
                AssetPairId = crossIndexAssetPairSettings.AssetPairId,
                Alpha = indexSettings.Alpha,
                TrackingFee = indexSettings.TrackingFee,
                PerformanceFee = indexSettings.PerformanceFee,
                SellMarkup = indexSettings.SellMarkup,
                SellVolume = indexSettings.SellVolume * crossRate,
                BuyVolume = indexSettings.BuyVolume * crossRate,
                SellLimitOrdersCount = indexSettings.SellLimitOrdersCount,
                BuyLimitOrdersCount = indexSettings.BuyLimitOrdersCount
            };

            return result;
        }

        private async Task<AssetPairSettings> GetCrossIndexAssetPairSettings(IndexSettings indexSettings, CrossIndexSettings crossIndexSettings)
        {
            AssetPairSettings indexAssetPairSettings =
                await _instrumentService.GetAssetPairAsync(indexSettings.AssetPairId, Exchange);

            if (indexAssetPairSettings == null)
                throw new InvalidOperationException("Index asset pair settings not found");

            AssetSettings indexBaseAssetSettings =
                await _instrumentService.GetAssetAsync(indexAssetPairSettings.BaseAsset, ExchangeNames.Lykke);

            if (indexBaseAssetSettings == null)
                throw new InvalidOperationException("Index base asset settings not found");

            AssetPairSettings crossAssetPairSettings =
                await _instrumentService.GetAssetPairAsync(crossIndexSettings.AssetPairId, crossIndexSettings.Exchange);

            if (crossAssetPairSettings == null)
                throw new InvalidOperationException("Cross asset pair settings not found");

            AssetSettings crossAssetPairBaseAssetSettings =
                await _instrumentService.GetAssetAsync(crossAssetPairSettings.BaseAsset, ExchangeNames.Lykke);

            if (crossAssetPairBaseAssetSettings == null)
                throw new InvalidOperationException("Cross base asset settings not found");

            AssetSettings crossAssetPairQuoteAssetSettings =
                await _instrumentService.GetAssetAsync(crossAssetPairSettings.QuoteAsset, ExchangeNames.Lykke);

            if (crossAssetPairQuoteAssetSettings == null)
                throw new InvalidOperationException("Cross quote asset settings not found");

            AssetSettings crossIndexBaseAssetSettings = indexBaseAssetSettings;

            AssetSettings crossIndexQuoteAssetSettings =
                crossIndexSettings.IsInverted ? crossAssetPairBaseAssetSettings : crossAssetPairQuoteAssetSettings;

            AssetPairSettings crossIndexAssetPairSettings = await _instrumentService.GetAssetPairAsync(
                crossIndexBaseAssetSettings.AssetId, crossIndexQuoteAssetSettings.AssetId, ExchangeNames.Lykke);

            if (crossIndexAssetPairSettings == null)
                throw new InvalidOperationException("Cross index asset pair settings not found");

            return crossIndexAssetPairSettings;
        }

        private async Task<IndexSettings> GetNewIndexSettingsForCrossIndex(IndexSettings indexSettings, CrossIndexSettings crossIndexSettings)
        {
            AssetPairSettings crossIndexAssetPairSettings = await GetCrossIndexAssetPairSettings(
                indexSettings, crossIndexSettings);

            var crossRate = GetCrossRate(crossIndexSettings);

            IndexSettings result = GetNewIndexSettingsForCrossIndex(indexSettings, crossIndexAssetPairSettings,
                crossRate);

            return result;
        }

        private decimal GetCrossRate(CrossIndexSettings crossIndexSettings)
        {
            Quote quote = _quoteService.GetByAssetPairId(crossIndexSettings.AssetPairId, crossIndexSettings.Exchange);

            if (quote == null)
                throw new InvalidOperationException("Cross index asset pair quote not found");

            decimal crossRate = crossIndexSettings.IsInverted ? 1 / quote.Mid : quote.Mid;

            return crossRate;
        }

        private async Task CancelCrossLimitOrdersAsync(IndexSettings indexSettings)
        {
            IReadOnlyList<CrossIndexSettings> indexCrossIndexSettings =
                await _crossIndexSettingsService.FindByIndexAssetPairAsync(indexSettings.AssetPairId);

            foreach (CrossIndexSettings crossIndexSettings in indexCrossIndexSettings)
            {
                AssetPairSettings crossIndexAssetPairSettings = await GetCrossIndexAssetPairSettings(indexSettings, crossIndexSettings);

                await _lykkeExchangeService.CancelAsync(crossIndexAssetPairSettings.AssetPairId);

                _log.InfoWithDetails("Cross index limit orders canceled", new { IndexName = indexSettings.Name, indexSettings.AssetPairId });
            }
        }
    }
}
