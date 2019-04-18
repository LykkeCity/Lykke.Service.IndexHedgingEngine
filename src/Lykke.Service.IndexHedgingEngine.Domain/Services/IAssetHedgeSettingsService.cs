using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IAssetHedgeSettingsService
    {
        Task<IReadOnlyCollection<AssetHedgeSettings>> GetAllAsync();

        Task<AssetHedgeSettings> GetByAssetIdAsync(string assetId);

        Task<AssetHedgeSettings> GetByAssetIdAsync(string assetId, string exchange);
        
        Task<AssetHedgeSettings> EnsureAsync(string assetId);

        Task AddAsync(AssetHedgeSettings assetHedgeSettings);

        Task UpdateAsync(AssetHedgeSettings assetHedgeSettings);

        Task DeleteAsync(string assetId);
    }
}
