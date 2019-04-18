using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IAssetPairSettingsRepository
    {
        Task<IReadOnlyCollection<AssetPairSettings>> GetAllAsync();

        Task InsertAsync(AssetPairSettings assetPairSettings);

        Task UpdateAsync(AssetPairSettings assetPairSettings);

        Task DeleteAsync(string assetPair, string exchange);
    }
}
