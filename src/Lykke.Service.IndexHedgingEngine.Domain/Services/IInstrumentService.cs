using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IInstrumentService
    {
        Task<IReadOnlyCollection<AssetSettings>> GetAssetsAsync();

        Task<IReadOnlyCollection<AssetPairSettings>> GetAssetPairsAsync();

        Task<AssetSettings> GetAssetAsync(string asset, string exchange);

        Task<AssetSettings> GetAssetByIdAsync(string assetId, string exchange = "lykke");

        Task<AssetPairSettings> GetAssetPairAsync(string assetPair, string exchange);

        Task<AssetPairSettings> GetAssetPairByIdAsync(string assetPairId, string exchange = "lykke");

        Task AddAssetAsync(AssetSettings assetSettings, string userId);

        Task AddAssetPairAsync(AssetPairSettings assetPairSettings, string userId);

        Task UpdateAssetAsync(AssetSettings assetSettings, string userId);

        Task UpdateAssetPairAsync(AssetPairSettings assetPairSettings, string userId);

        Task DeleteAssetAsync(string asset, string exchange, string userId);

        Task DeleteAssetPairAsync(string assetPair, string exchange, string userId);

        Task<bool> IsAssetPairExistAsync(string assetPair);
    }
}
