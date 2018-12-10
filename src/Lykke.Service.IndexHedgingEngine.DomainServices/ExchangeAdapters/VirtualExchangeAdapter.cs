using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Infrastructure;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.ExchangeAdapters
{
    public class VirtualExchangeAdapter : IExchangeAdapter
    {
        private readonly IPositionService _positionService;
        private readonly IHedgeLimitOrderService _hedgeLimitOrderService;
        private readonly IVirtualTradeService _virtualTradeService;

        public VirtualExchangeAdapter(
            IPositionService positionService,
            IHedgeLimitOrderService hedgeLimitOrderService,
            IVirtualTradeService virtualTradeService)
        {
            _positionService = positionService;
            _hedgeLimitOrderService = hedgeLimitOrderService;
            _virtualTradeService = virtualTradeService;
        }

        public string Name => ExchangeNames.Virtual;

        public Task CancelLimitOrderAsync(string assetId)
        {
            _hedgeLimitOrderService.Close(assetId);
            
            return Task.CompletedTask;
        }

        public async Task ExecuteLimitOrderAsync(HedgeLimitOrder hedgeLimitOrder)
        {
            await _hedgeLimitOrderService.AddAsync(hedgeLimitOrder);
            
            VirtualTrade virtualTrade = VirtualTrade.Create(
                hedgeLimitOrder.Id,
                hedgeLimitOrder.AssetId,
                hedgeLimitOrder.AssetPairId,
                hedgeLimitOrder.Type == LimitOrderType.Sell
                    ? TradeType.Sell
                    : TradeType.Buy,
                hedgeLimitOrder.Timestamp,
                hedgeLimitOrder.Price,
                hedgeLimitOrder.Volume);

            await _positionService.UpdateAsync(virtualTrade.AssetId, Name, virtualTrade.Type, virtualTrade.Volume,
                virtualTrade.OppositeVolume);

            await _virtualTradeService.AddAsync(virtualTrade);
            
            _hedgeLimitOrderService.Close(hedgeLimitOrder.AssetId);
        }
    }
}
