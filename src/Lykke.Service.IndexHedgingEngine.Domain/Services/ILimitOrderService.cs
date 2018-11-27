using System.Collections.Generic;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ILimitOrderService
    {
        IReadOnlyCollection<OrderBook> GetAll();

        OrderBook GetByAssetPairId(string assetPairId);

        void Update(string assetPairId, IReadOnlyCollection<LimitOrder> limitOrders);
    }
}
