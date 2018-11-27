using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Handlers;
using Lykke.Service.IndexHedgingEngine.Domain.Infrastructure;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.ExchangeAdapters
{
    public class LykkeExchangeAdapter : IExchangeAdapter, IInternalHedgeTradeHandler
    {
        private readonly IPositionService _positionService;
        private readonly ILykkeTradeService _lykkeTradeService;
        private readonly IHedgeLimitOrderService _hedgeLimitOrderService;
        private readonly ILykkeExchangeService _lykkeExchangeService;

        private readonly IDictionary<string, HedgeLimitOrder> _hedgeLimitOrders =
            new Dictionary<string, HedgeLimitOrder>();

        public LykkeExchangeAdapter(
            IPositionService positionService,
            ILykkeTradeService lykkeTradeService,
            IHedgeLimitOrderService hedgeLimitOrderService,
            ILykkeExchangeService lykkeExchangeService)
        {
            _positionService = positionService;
            _lykkeTradeService = lykkeTradeService;
            _hedgeLimitOrderService = hedgeLimitOrderService;
            _lykkeExchangeService = lykkeExchangeService;
        }

        public string Name => ExchangeNames.Lykke;

        public async Task CancelLimitOrderAsync(string assetId)
        {
            if (_hedgeLimitOrders.TryGetValue(assetId, out HedgeLimitOrder hedgeLimitOrder))
            {
                await _lykkeExchangeService.ApplyAsync(hedgeLimitOrder.AssetPairId, new LimitOrder[0]);

                _hedgeLimitOrders.Remove(assetId);
            }
        }

        public async Task ExecuteLimitOrderAsync(HedgeLimitOrder hedgeLimitOrder)
        {
            _hedgeLimitOrders[hedgeLimitOrder.AssetId] = hedgeLimitOrder;

            await _lykkeExchangeService.ApplyAsync(hedgeLimitOrder.AssetPairId, new[]
            {
                new LimitOrder
                {
                    Id = hedgeLimitOrder.Id,
                    Price = hedgeLimitOrder.Price,
                    Volume = hedgeLimitOrder.Volume,
                    Type = hedgeLimitOrder.Type
                }
            });
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
                    continue;

                await _lykkeTradeService.RegisterAsync(internalTrade);

                await _positionService.UpdateAsync(hedgeLimitOrder.AssetId, Name, internalTrade.Type,
                    internalTrade.Volume, internalTrade.OppositeVolume);
            }
        }
    }
}
