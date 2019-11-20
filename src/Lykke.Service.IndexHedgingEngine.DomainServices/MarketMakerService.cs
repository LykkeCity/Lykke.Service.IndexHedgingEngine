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
using Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm;
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
        private readonly ICrossAssetPairSettingsService _crossAssetPairSettingsService;
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
            ICrossAssetPairSettingsService crossAssetPairSettingsService,
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
            _crossAssetPairSettingsService = crossAssetPairSettingsService;
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

            AssetPairSettings assetPairSettings =
                await _instrumentService.GetAssetPairAsync(indexSettings.AssetPairId, Exchange);

            if (assetPairSettings == null)
                throw new InvalidOperationException("Asset pair settings not found");

            AssetSettings baseAssetSettings =
                await _instrumentService.GetAssetAsync(assetPairSettings.BaseAsset, ExchangeNames.Lykke);

            if (baseAssetSettings == null)
                throw new InvalidOperationException("Base asset settings not found");

            AssetSettings quoteAssetSettings =
                await _instrumentService.GetAssetAsync(assetPairSettings.QuoteAsset, ExchangeNames.Lykke);

            if (quoteAssetSettings == null)
                throw new InvalidOperationException("Quote asset settings not found");

            decimal sellPrice = (indexPrice.Price + indexSettings.SellMarkup)
                .TruncateDecimalPlaces(assetPairSettings.PriceAccuracy, true);

            decimal buyPrice = indexPrice.Price.TruncateDecimalPlaces(assetPairSettings.PriceAccuracy);

            IReadOnlyCollection<LimitOrder> limitOrders =
                CreateLimitOrders(indexSettings, assetPairSettings, sellPrice, buyPrice);

            ValidateBalance(limitOrders, baseAssetSettings, quoteAssetSettings);

            ValidateMinVolume(limitOrders, assetPairSettings.MinVolume);

            LimitOrder[] allowedLimitOrders = limitOrders
                .Where(o => o.Error == LimitOrderError.None)
                .ToArray();

            _log.InfoWithDetails("Limit orders created", limitOrders);

            _limitOrderService.Update(indexSettings.AssetPairId, limitOrders);

            await _lykkeExchangeService.ApplyAsync(indexSettings.AssetPairId, allowedLimitOrders);

            _traceWriter.LimitOrders(indexSettings.AssetPairId, limitOrders);
        }

        public async Task UpdateCrossPairLimitOrdersAsync(CrossAssetPairSettings crossAssetPairSettings)
        {
            var allAssetPairSettings = await _instrumentService.GetAssetPairsAsync();

            var assetPairSettings = allAssetPairSettings.FirstOrDefault(x => x.BaseAsset == crossAssetPairSettings.BaseAsset
                                                                          && x.QuoteAsset == crossAssetPairSettings.QuoteAsset);

            if (assetPairSettings == null)
                throw new InvalidOperationException("Asset pair settings for the cross pair is not found");

            AssetSettings baseAssetSettings =
                await _instrumentService.GetAssetAsync(crossAssetPairSettings.BaseAsset, ExchangeNames.Lykke);

            if (baseAssetSettings == null)
                throw new InvalidOperationException("Base asset settings for the cross pair is not found");

            AssetSettings quoteAssetSettings =
                await _instrumentService.GetAssetAsync(crossAssetPairSettings.QuoteAsset, ExchangeNames.Lykke);

            if (quoteAssetSettings == null)
                throw new InvalidOperationException("Quote asset settings for the cross pair is not found");

            var allIndexPrices = await _indexPriceService.GetAllAsync();

            var allQuotes = _quoteService.GetAll();

            var price = CrossAssetPairPriceCalculator.Calculate(crossAssetPairSettings, allIndexPrices,
                allAssetPairSettings, allQuotes, out var priceErrorMessage);

            if (price == null || price.Value <= 0)
            {
                var errorMessage = "Can't calculate a price for the cross pair";

                _log.WarningWithDetails(errorMessage, new { priceErrorMessage, crossAssetPairSettings });

                throw new InvalidOperationException(errorMessage);
            }
            
            decimal sellPrice = (price.Value * (1 + crossAssetPairSettings.SellSpread))
                .TruncateDecimalPlaces(assetPairSettings.PriceAccuracy, true);

            decimal buyPrice = (price.Value * (1 - crossAssetPairSettings.BuySpread))
                .TruncateDecimalPlaces(assetPairSettings.PriceAccuracy);

            var limitOrders = CreateCrossPairLimitOrders(crossAssetPairSettings, assetPairSettings, sellPrice, buyPrice);

            // validate balance and min volume

            ValidateBalance(limitOrders, baseAssetSettings, quoteAssetSettings);

            ValidateMinVolume(limitOrders, assetPairSettings.MinVolume);

            // update valid limit orders and apply them to Matching Engine

            LimitOrder[] allowedLimitOrders = limitOrders
                .Where(o => o.Error == LimitOrderError.None)
                .ToArray();

            _log.InfoWithDetails("Limit orders for the cross pair created", limitOrders);

            _limitOrderService.Update(assetPairSettings.AssetPairId, limitOrders);

            await _lykkeExchangeService.ApplyAsync(assetPairSettings.AssetPairId, allowedLimitOrders);

            _traceWriter.LimitOrders(assetPairSettings.AssetPairId, limitOrders);
        }

        public async Task CancelLimitOrdersAsync(string indexName)
        {
            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(indexName);

            if (indexSettings == null)
                throw new InvalidOperationException("Index settings not found");

            await _lykkeExchangeService.CancelAsync(indexSettings.AssetPairId);

            // find all cross pairs with the index and cancel their limit orders

            var indexCrossPairs = await _crossAssetPairSettingsService.FindCrossAssetPairsByIndexAsync(indexName, null);

            foreach (var crossAssetPairSettings in indexCrossPairs)
            {
                await CancelCrossPairLimitOrdersAsync(crossAssetPairSettings);
            }

            _log.InfoWithDetails("Limit orders canceled", new {IndexName = indexName, indexSettings.AssetPairId});
        }

        public async Task CancelCrossPairLimitOrdersAsync(CrossAssetPairSettings crossAssetPairSettings)
        {
            var allAssetPairSettings = await _instrumentService.GetAssetPairsAsync();

            var assetPairSettings = allAssetPairSettings.FirstOrDefault(x => x.BaseAsset == crossAssetPairSettings.BaseAsset
                                                                             && x.QuoteAsset == crossAssetPairSettings.QuoteAsset);

            if (assetPairSettings == null)
                throw new InvalidOperationException("Asset pair settings for the cross pair is not found");

            await _lykkeExchangeService.CancelAsync(assetPairSettings.AssetPairId);
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

        private IReadOnlyCollection<LimitOrder> CreateCrossPairLimitOrders(CrossAssetPairSettings crossAssetPairSettings,
            AssetPairSettings assetPairSettings, decimal sellPrice, decimal buyPrice)
        {
            var limitOrders = new List<LimitOrder>();

            string walletId = _settingsService.GetWalletId();

            decimal sellVolume = Math.Round(crossAssetPairSettings.SellVolume, assetPairSettings.VolumeAccuracy);
            
            limitOrders.Add(LimitOrder.CreateSell(walletId, sellPrice, sellVolume));

            decimal buyVolume = Math.Round(crossAssetPairSettings.SellVolume, assetPairSettings.VolumeAccuracy);

            limitOrders.Add(LimitOrder.CreateBuy(walletId, buyPrice, buyVolume));

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
    }
}
