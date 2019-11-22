using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ICrossAssetPairSettingsService
    {
        Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAllAsync();

        Task<IReadOnlyCollection<CrossAssetPairSettings>> FindEnabledByIndexAsync(string indexName, string shortIndexName);

        Task<CrossAssetPairSettings> FindByBaseAndQuoteAssetsAsync(string baseAsset, string quoteAsset);

        Task<CrossAssetPairSettings> FindByIdAsync(Guid id);

        Task AddCrossAssetPairAsync(CrossAssetPairSettings crossAssetPairSettings, string userId);

        Task UpdateCrossAssetPairAsync(CrossAssetPairSettings crossAssetPairSettings, string userId);

        Task UpdateModeAsync(Guid id, CrossAssetPairSettingsMode mode, string userId);

        Task DeleteCrossAssetPairAsync(Guid id, string userId);
    }
}
