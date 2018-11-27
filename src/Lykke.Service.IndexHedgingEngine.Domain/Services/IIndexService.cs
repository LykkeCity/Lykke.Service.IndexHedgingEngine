using System.Collections.Generic;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IIndexService
    {
        IReadOnlyCollection<Index> GetAll();
        
        Index Get(string indexName);

        void Update(Index index);
    }
}
