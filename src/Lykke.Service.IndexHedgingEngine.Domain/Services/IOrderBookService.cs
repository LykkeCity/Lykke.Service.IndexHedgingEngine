using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IOrderBookService
    {
        IReadOnlyCollection<OrderBook> GetAll();

        IReadOnlyCollection<OrderBook> Get(int limit);
        
        Task UpdateSellLimitOrdersAsync(string assetPairId, DateTime timestamp,
            IReadOnlyCollection<LimitOrder> limitOrders);

        Task UpdateBuyLimitOrdersAsync(string assetPairId, DateTime timestamp,
            IReadOnlyCollection<LimitOrder> limitOrders);
    }
}
