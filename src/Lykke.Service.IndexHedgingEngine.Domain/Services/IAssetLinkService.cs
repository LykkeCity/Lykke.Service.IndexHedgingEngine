using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IAssetLinkService
    {
        Task<IReadOnlyCollection<AssetLink>> GetAllAsync();

        Task<AssetLink> GetByAssetIdAsync(string assetId);

        Task<IReadOnlyCollection<string>> GetMissedAsync();

        Task AddAsync(AssetLink assetLink);

        Task UpdateAsync(AssetLink assetLink);

        Task DeleteAsync(string assetId);
    }
}
