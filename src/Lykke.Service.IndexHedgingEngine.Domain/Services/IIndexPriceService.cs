using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IIndexPriceService
    {
        Task<IReadOnlyCollection<IndexPrice>> GetAllAsync();

        Task<IndexPrice> GetByIndexAsync(string indexName);

        Task UpdateAsync(Index index);
    }
}
