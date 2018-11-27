using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IPositionRepository
    {
        Task<IReadOnlyCollection<Position>> GetAllAsync();

        Task<Position> GetByAssetIdAsync(string assetId, string exchange);

        Task InsertAsync(Position position);

        Task UpdateAsync(Position position);

        Task DeleteAsync(string assetId, string exchange);
    }
}
