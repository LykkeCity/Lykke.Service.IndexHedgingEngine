using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Hedging
{
    public class HedgeLimitOrderService : IHedgeLimitOrderService
    {
        private readonly IHedgeLimitOrderRepository _hedgeLimitOrderRepository;
        private readonly InMemoryCache<HedgeLimitOrder> _cache;

        public HedgeLimitOrderService(IHedgeLimitOrderRepository hedgeLimitOrderRepository)
        {
            _hedgeLimitOrderRepository = hedgeLimitOrderRepository;
            _cache = new InMemoryCache<HedgeLimitOrder>(GetKey, true);
        }

        public IReadOnlyCollection<HedgeLimitOrder> GetAll()
        {
            return _cache.GetAll();
        }

        public Task<HedgeLimitOrder> GetByIdAsync(string hedgeLimitOrderId)
        {
            return _hedgeLimitOrderRepository.GetByIdAsync(hedgeLimitOrderId);
        }
        
        public async Task UpdateAsync(IReadOnlyCollection<HedgeLimitOrder> hedgeLimitOrders)
        {
            _cache.Clear();
            _cache.Set(hedgeLimitOrders);

            await _hedgeLimitOrderRepository.InsertAsync(hedgeLimitOrders);
        }

        private static string GetKey(HedgeLimitOrder hedgeLimitOrder)
            => GetKey(hedgeLimitOrder.AssetId);

        private static string GetKey(string assetId)
            => assetId.ToUpper();
    }
}
