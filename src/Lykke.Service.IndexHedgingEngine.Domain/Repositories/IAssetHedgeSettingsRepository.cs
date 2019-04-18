using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IAssetHedgeSettingsRepository
    {
        Task<IReadOnlyCollection<AssetHedgeSettings>> GetAllAsync();

        Task<AssetHedgeSettings> GetByAssetIdAsync(string assetId);

        Task InsertAsync(AssetHedgeSettings assetHedgeSettings);

        Task UpdateAsync(AssetHedgeSettings assetHedgeSettings);

        Task DeleteAsync(string assetId);
    }
}
