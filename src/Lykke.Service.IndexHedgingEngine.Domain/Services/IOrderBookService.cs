using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IOrderBookService
    {
        Task<IReadOnlyCollection<OrderBook>> GetAllAsync();

        Task<IReadOnlyCollection<OrderBook>> GetAsync(int limit);
        
        Task UpdateSellLimitOrdersAsync(string assetPairId, DateTime timestamp,
            IReadOnlyCollection<LimitOrder> limitOrders);

        Task UpdateBuyLimitOrdersAsync(string assetPairId, DateTime timestamp,
            IReadOnlyCollection<LimitOrder> limitOrders);
    }
}
