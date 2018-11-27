using System.Collections.Generic;
using System.Linq;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Indices
{
    [UsedImplicitly]
    public class IndexService : IIndexService
    {
        private readonly InMemoryCache<Index> _cache;

        private readonly ILog _log;

        public IndexService(ILogFactory logFactory)
        {
            _cache = new InMemoryCache<Index>(GetKey, true);

            _log = logFactory.CreateLog(this);
        }

        public IReadOnlyCollection<Index> GetAll()
        {
            return _cache.GetAll();
        }
        
        public Index Get(string indexName)
        {
            return _cache.Get(GetKey(indexName));
        }

        public void Update(Index index)
        {
            if (index.Weights.Sum(o => o.Weight) > 1.1m)
                _log.WarningWithDetails("Wrong weight in the index", index);

            _cache.Set(index);
        }

        private static string GetKey(Index index)
            => GetKey(index.Name);

        private static string GetKey(string indexName)
            => indexName.ToUpper();
    }
}
