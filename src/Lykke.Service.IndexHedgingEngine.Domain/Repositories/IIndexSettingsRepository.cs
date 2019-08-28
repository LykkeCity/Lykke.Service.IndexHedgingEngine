using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IIndexSettingsRepository
    {
        Task<IReadOnlyCollection<IndexSettings>> GetAllAsync();

        Task InsertAsync(IndexSettings indexSettings);

        Task UpdateAsync(IndexSettings indexSettings);

        Task DeleteAsync(string indexName);
    }
}
