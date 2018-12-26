using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Infrastructure;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.ExchangeAdapters
{
    public class VirtualExchangeAdapter : IExchangeAdapter
    {
        private readonly IPositionService _positionService;
        private readonly IVirtualTradeService _virtualTradeService;
        private readonly ILog _log;

        public VirtualExchangeAdapter(
            IPositionService positionService,
            IVirtualTradeService virtualTradeService,
            ILogFactory logFactory)
        {
            _positionService = positionService;
            _virtualTradeService = virtualTradeService;
            _log = logFactory.CreateLog(this);
        }

        public string Name => ExchangeNames.Virtual;

        public Task CancelLimitOrderAsync(string assetId)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteLimitOrderAsync(HedgeLimitOrder hedgeLimitOrder)
        {
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

            _log.InfoWithDetails("Virtual trade executed", virtualTrade);
        }
    }
}
