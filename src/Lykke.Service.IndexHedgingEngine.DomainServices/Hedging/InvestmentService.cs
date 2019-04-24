using System.Collections.Generic;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Hedging
{
    public class InvestmentService : IInvestmentService
    {
        private readonly InMemoryCache<AssetInvestment> _cache;

        public InvestmentService()
        {
            _cache = new InMemoryCache<AssetInvestment>(o => o.AssetId, true);
        }

        public IReadOnlyCollection<AssetInvestment> GetAll()
        {
            return _cache.GetAll();
        }

        public void Update(IReadOnlyCollection<AssetInvestment> assetInvestments)
        {
            _cache.Clear();
            _cache.Set(assetInvestments);
        }
    }
}
