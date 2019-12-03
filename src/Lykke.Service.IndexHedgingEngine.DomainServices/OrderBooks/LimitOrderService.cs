using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.OrderBooks
{
    [UsedImplicitly]
    public class LimitOrderService : ILimitOrderService
    {
        private readonly InMemoryCache<OrderBook> _cache;

        public LimitOrderService()
        {
            _cache = new InMemoryCache<OrderBook>(GetKey, true);
        }

        public IReadOnlyCollection<OrderBook> GetAll()
        {
            return _cache.GetAll();
        }

        public OrderBook GetByAssetPairId(string assetPairId)
        {
            return _cache.Get(GetKey(assetPairId));
        }

        public void Update(string assetPairId, IReadOnlyCollection<LimitOrder> limitOrders)
        {
            _cache.Set(new OrderBook
            {
                AssetPairId = assetPairId,
                Timestamp = DateTime.UtcNow,
                LimitOrders = limitOrders
            });
        }

        public void Remove(string assetPairId)
        {
            _cache.Remove(assetPairId);
        }

        private static string GetKey(OrderBook orderBook)
            => GetKey(orderBook.AssetPairId);

        private static string GetKey(string assetPairId)
            => assetPairId.ToUpper();
    }
}
