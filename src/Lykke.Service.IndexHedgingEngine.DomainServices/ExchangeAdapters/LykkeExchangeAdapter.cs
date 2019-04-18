using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Handlers;
using Lykke.Service.IndexHedgingEngine.Domain.Infrastructure;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.ExchangeAdapters
{
    public class LykkeExchangeAdapter : IExchangeAdapter, IInternalTradeHandler
    {
        private readonly IPositionService _positionService;
        private readonly ILykkeTradeService _lykkeTradeService;
        private readonly IHedgeLimitOrderService _hedgeLimitOrderService;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly IInstrumentService _instrumentService;
        private readonly ILog _log;

        private readonly IDictionary<string, HedgeLimitOrder> _hedgeLimitOrders =
            new Dictionary<string, HedgeLimitOrder>();

        public LykkeExchangeAdapter(
            IPositionService positionService,
            ILykkeTradeService lykkeTradeService,
            IHedgeLimitOrderService hedgeLimitOrderService,
            ILykkeExchangeService lykkeExchangeService,
            IInstrumentService instrumentService,
            ILogFactory logFactory)
        {
            _positionService = positionService;
            _lykkeTradeService = lykkeTradeService;
            _hedgeLimitOrderService = hedgeLimitOrderService;
            _lykkeExchangeService = lykkeExchangeService;
            _instrumentService = instrumentService;
            _log = logFactory.CreateLog(this);
        }

        public string Name => ExchangeNames.Lykke;

        public async Task CancelLimitOrderAsync(string assetId)
        {
            if (_hedgeLimitOrders.TryGetValue(assetId, out HedgeLimitOrder hedgeLimitOrder))
            {
                await _lykkeExchangeService.CancelAsync(hedgeLimitOrder.AssetPairId);

                _hedgeLimitOrderService.Close(hedgeLimitOrder);
                
                _hedgeLimitOrders.Remove(assetId);
            }
        }

        public async Task ExecuteLimitOrderAsync(HedgeLimitOrder hedgeLimitOrder)
        {
            await _hedgeLimitOrderService.AddAsync(hedgeLimitOrder);

            AssetPairSettings assetPairSettings =
                await _instrumentService.GetAssetPairAsync(hedgeLimitOrder.AssetPairId, Name);

            if (assetPairSettings == null)
            {
                hedgeLimitOrder.Error = LimitOrderError.Unknown;
                hedgeLimitOrder.ErrorMessage = "Instrument not configured";

                _log.WarningWithDetails("No settings for instrument", hedgeLimitOrder);
                
                return;
            }
            
            decimal price = hedgeLimitOrder.Price
                .TruncateDecimalPlaces(assetPairSettings.PriceAccuracy, hedgeLimitOrder.Type == LimitOrderType.Sell);

            decimal volume = Math.Round(hedgeLimitOrder.Volume, assetPairSettings.VolumeAccuracy);

            if (volume < assetPairSettings.MinVolume)
            {
                hedgeLimitOrder.Error = LimitOrderError.TooSmallVolume;
                return;
            }

            var limitOrder = new LimitOrder
            {
                Id = hedgeLimitOrder.Id,
                Price = price,
                Volume = volume,
                Type = hedgeLimitOrder.Type
            };

            try
            {
                await _lykkeExchangeService.ApplyAsync(hedgeLimitOrder.AssetPairId, limitOrder);

                if(limitOrder.Error == LimitOrderError.None)
                    _hedgeLimitOrders[hedgeLimitOrder.AssetId] = hedgeLimitOrder;
                
                hedgeLimitOrder.Error = limitOrder.Error;
                hedgeLimitOrder.ErrorMessage = limitOrder.ErrorMessage;
            }
            catch (ExchangeException exception)
            {
                hedgeLimitOrder.Error = LimitOrderError.Unknown;
                hedgeLimitOrder.ErrorMessage = exception.Message;
            }
            catch (Exception exception)
            {
                hedgeLimitOrder.Error = LimitOrderError.Unknown;
                hedgeLimitOrder.ErrorMessage = "Cannot create limit orders an unexpected error occurred";

                _log.WarningWithDetails("An error occurred during creating limit orders", exception, hedgeLimitOrder);
            }
        }

        public async Task HandleInternalTradesAsync(IReadOnlyCollection<InternalTrade> internalTrades)
        {
            foreach (InternalTrade internalTrade in internalTrades)
            {
                HedgeLimitOrder hedgeLimitOrder =
                    await _hedgeLimitOrderService.GetByIdAsync(internalTrade.LimitOrderId);

                if (hedgeLimitOrder == null)
                    continue;

                if (await _lykkeTradeService.ExistAsync(internalTrade.Id))
                {
                    _log.WarningWithDetails("Trade already registered.", internalTrade);
                    continue;
                }

                await _lykkeTradeService.RegisterAsync(internalTrade);

                await _positionService.UpdateAsync(hedgeLimitOrder.AssetId, Name, internalTrade.Type,
                    internalTrade.Volume, internalTrade.OppositeVolume);

                hedgeLimitOrder.ExecuteVolume(internalTrade.Volume);

                if (internalTrade.Status == TradeStatus.Fill)
                    _hedgeLimitOrderService.Close(hedgeLimitOrder);
            }
        }
    }
}
