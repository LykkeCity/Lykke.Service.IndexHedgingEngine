using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IIndexPriceRepository
    {
        Task<IReadOnlyCollection<IndexPrice>> GetAllAsync();

        Task<IndexPrice> GetByIndexAsync(string indexName);

        Task InsertAsync(IndexPrice indexPrice);

        Task UpdateAsync(IndexPrice indexPrice);

        Task DeleteAsync(string indexName);
    }
}
