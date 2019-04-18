using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IInstrumentService
    {
        Task<IReadOnlyCollection<AssetSettings>> GetAssetsAsync();

        Task<IReadOnlyCollection<AssetPairSettings>> GetAssetPairsAsync();

        Task<AssetSettings> GetAssetAsync(string asset, string exchange);

        Task<AssetPairSettings> GetAssetPairAsync(string assetPair, string exchange);

        Task AddAssetAsync(AssetSettings assetSettings, string userId);

        Task AddAssetPairAsync(AssetPairSettings assetPairSettings, string userId);

        Task UpdateAssetAsync(AssetSettings assetSettings, string userId);

        Task UpdateAssetPairAsync(AssetPairSettings assetPairSettings, string userId);

        Task DeleteAssetAsync(string asset, string exchange, string userId);

        Task DeleteAssetPairAsync(string assetPair, string exchange, string userId);

        Task<bool> IsAssetPairExistAsync(string assetPair);
    }
}
