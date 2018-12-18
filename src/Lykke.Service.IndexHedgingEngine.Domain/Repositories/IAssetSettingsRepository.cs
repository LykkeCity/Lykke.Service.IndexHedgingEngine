using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IAssetSettingsRepository
    {
        Task<IReadOnlyCollection<AssetSettings>> GetAllAsync();

        Task InsertAsync(AssetSettings assetSettings);
        
        Task UpdateAsync(AssetSettings assetSettings);
        
        Task DeleteAsync(string asset, string exchange);
    }
}
