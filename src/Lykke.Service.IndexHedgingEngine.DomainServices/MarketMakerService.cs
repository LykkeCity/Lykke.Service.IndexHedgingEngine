using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client.Models.v3;
using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices
{
    [UsedImplicitly]
    public class MarketMakerService : IMarketMakerService
    {
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IIndexPriceService _indexPriceService;
        private readonly IBalanceService _balanceService;
        private readonly ISettingsService _settingsService;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly ILimitOrderService _limitOrderService;
        private readonly IAssetsReadModelRepository _assetsReadModelRepository;
        private readonly IAssetPairsReadModelRepository _assetPairsReadModelRepository;

        private readonly ILog _log;

        public MarketMakerService(
            IIndexService indexService,
            IIndexSettingsService indexSettingsService,
            IIndexPriceService indexPriceService,
            IBalanceService balanceService,
            ISettingsService settingsService,
            ILykkeExchangeService lykkeExchangeService,
            ILimitOrderService limitOrderService,
            IAssetsReadModelRepository assetsReadModelRepository,
            IAssetPairsReadModelRepository assetPairsReadModelRepository,
            ILogFactory logFactory)
        {
            _indexSettingsService = indexSettingsService;
            _indexPriceService = indexPriceService;
            _balanceService = balanceService;
            _settingsService = settingsService;
            _lykkeExchangeService = lykkeExchangeService;
            _limitOrderService = limitOrderService;
            _assetsReadModelRepository = assetsReadModelRepository;
            _assetPairsReadModelRepository = assetPairsReadModelRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task UpdateOrderBookAsync(Index index)
        {
            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(index.Name);

            if (indexSettings == null)
                return;

            IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(index.Name);

            if (indexPrice == null)
            {
                indexPrice = IndexPrice.Init(index.Name, index.Value, index.Timestamp, index.Weights);

                await _indexPriceService.AddAsync(indexPrice);

                _log.InfoWithDetails("The index state initialized", indexPrice);

                return;
            }

            LimitOrderPrice limitOrderPrice = LimitOrderPriceCalculator.CalculatePrice(
                index.Value, indexPrice.Value, indexSettings.Alpha, indexPrice.K, indexPrice.Price,
                index.Timestamp, indexPrice.Timestamp, indexSettings.TrackingFee, indexSettings.PerformanceFee);

            indexPrice.Update(index.Value, index.Timestamp, limitOrderPrice.Price, limitOrderPrice.K, limitOrderPrice.R,
                limitOrderPrice.Delta, index.Weights);

            await _indexPriceService.UpdateAsync(indexPrice);

            _log.InfoWithDetails("The index price calculated", new
            {
                indexPrice,
                indexSettings
            });

            Asset asset = _assetsReadModelRepository.TryGet(indexSettings.AssetId);
            AssetPair assetPair = _assetPairsReadModelRepository.TryGet(indexSettings.AssetPairId);

            string walletId = await _settingsService.GetWalletIdAsync();

            var limitOrders = new[]
            {
                LimitOrder.CreateSell(walletId,
                    (limitOrderPrice.Price + indexSettings.SellMarkup).TruncateDecimalPlaces(assetPair.Accuracy, true),
                    Math.Round(indexSettings.SellVolume, asset.Accuracy)),
                LimitOrder.CreateBuy(walletId, limitOrderPrice.Price.TruncateDecimalPlaces(assetPair.Accuracy),
                    Math.Round(indexSettings.BuyVolume, asset.Accuracy))
            };

            ValidateMinVolume(limitOrders, assetPair.MinVolume);

            await ValidateBalanceAsync(limitOrders, assetPair);

            LimitOrder[] allowedLimitOrders = limitOrders
                .Where(o => o.Error == LimitOrderError.None)
                .ToArray();

            _log.InfoWithDetails("Limit orders created", limitOrders);

            _limitOrderService.Update(indexSettings.AssetPairId, limitOrders);

            await _lykkeExchangeService.ApplyAsync(indexSettings.AssetPairId, allowedLimitOrders);
        }

        private async Task ValidateBalanceAsync(IReadOnlyCollection<LimitOrder> limitOrders, AssetPair assetPair)
        {
            Asset baseAsset = _assetsReadModelRepository.TryGet(assetPair.BaseAssetId);
            Asset quoteAsset = _assetsReadModelRepository.TryGet(assetPair.QuotingAssetId);

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
                decimal balance = (await _balanceService.GetByAssetIdAsync(ExchangeNames.Lykke, baseAsset.Id)).Amount;

                foreach (LimitOrder limitOrder in sellLimitOrders)
                {
                    decimal amount = limitOrder.Volume.TruncateDecimalPlaces(baseAsset.Accuracy, true);

                    if (balance - amount < 0)
                    {
                        decimal volume = balance.TruncateDecimalPlaces(baseAsset.Accuracy);

                        if (volume < assetPair.MinVolume)
                            limitOrder.Error = LimitOrderError.NotEnoughFunds;
                        else
                            limitOrder.UpdateVolume(volume);
                    }

                    balance -= amount;
                }
            }

            if (buyLimitOrders.Any())
            {
                decimal balance = (await _balanceService.GetByAssetIdAsync(ExchangeNames.Lykke, quoteAsset.Id)).Amount;

                foreach (LimitOrder limitOrder in buyLimitOrders)
                {
                    decimal amount = (limitOrder.Price * limitOrder.Volume)
                        .TruncateDecimalPlaces(quoteAsset.Accuracy, true);

                    if (balance - amount < 0)
                    {
                        decimal volume = (balance / limitOrder.Price).TruncateDecimalPlaces(baseAsset.Accuracy);

                        if (volume < assetPair.MinVolume)
                            limitOrder.Error = LimitOrderError.NotEnoughFunds;
                        else
                            limitOrder.UpdateVolume(volume);
                    }

                    balance -= amount;
                }
            }
        }

        private static void ValidateMinVolume(IEnumerable<LimitOrder> limitOrders, decimal minVolume)
        {
            foreach (LimitOrder limitOrder in limitOrders.Where(o => o.Error == LimitOrderError.None))
            {
                if (limitOrder.Volume < minVolume)
                    limitOrder.Error = LimitOrderError.TooSmallVolume;
            }
        }
    }
}
