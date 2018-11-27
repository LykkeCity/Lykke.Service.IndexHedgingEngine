using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IAssetLinkRepository
    {
        Task<IReadOnlyCollection<AssetLink>> GetAllAsync();

        Task InsertAsync(AssetLink assetLink);

        Task UpdateAsync(AssetLink assetLink);

        Task DeleteAsync(string assetId);
    }
}
