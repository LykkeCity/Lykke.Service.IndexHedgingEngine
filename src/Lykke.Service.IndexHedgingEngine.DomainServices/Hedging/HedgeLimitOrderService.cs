using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Hedging
{
    public class HedgeLimitOrderService : IHedgeLimitOrderService
    {
        private readonly IHedgeLimitOrderRepository _hedgeLimitOrderRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly InMemoryCache<HedgeLimitOrder> _cache;

        public HedgeLimitOrderService(
            IHedgeLimitOrderRepository hedgeLimitOrderRepository,
            IMemoryCache memoryCache)
        {
            _hedgeLimitOrderRepository = hedgeLimitOrderRepository;
            _memoryCache = memoryCache;
            _cache = new InMemoryCache<HedgeLimitOrder>(GetKey, true);
        }

        public IReadOnlyCollection<HedgeLimitOrder> GetAll()
        {
            return _cache.GetAll();
        }

        public async Task<HedgeLimitOrder> GetByIdAsync(string hedgeLimitOrderId)
        {
            var hedgeLimitOrder = _memoryCache.Get<HedgeLimitOrder>(hedgeLimitOrderId);

            if (hedgeLimitOrder != null)
                return hedgeLimitOrder;
            
            hedgeLimitOrder = await _hedgeLimitOrderRepository.GetByIdAsync(hedgeLimitOrderId);

            if (hedgeLimitOrder != null)
            {
                _memoryCache.Set(hedgeLimitOrder.Id, hedgeLimitOrder,
                    new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)});
            }

            return hedgeLimitOrder;
        }
        
        public async Task AddAsync(HedgeLimitOrder hedgeLimitOrder)
        {
            _cache.Set(hedgeLimitOrder);

            _memoryCache.Set(hedgeLimitOrder.Id, hedgeLimitOrder,
                new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)});
            
            await _hedgeLimitOrderRepository.InsertAsync(hedgeLimitOrder);
        }

        public void Close(string assetId)
        {
            _cache.Remove(GetKey(assetId));
        }

        private static string GetKey(HedgeLimitOrder hedgeLimitOrder)
            => GetKey(hedgeLimitOrder.AssetId);

        private static string GetKey(string assetId)
            => assetId.ToUpper();
    }
}
