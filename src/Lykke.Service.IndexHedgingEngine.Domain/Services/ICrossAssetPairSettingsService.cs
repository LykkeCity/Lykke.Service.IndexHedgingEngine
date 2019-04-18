using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ICrossAssetPairSettingsService
    {
        Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAllAsync();

        Task<CrossAssetPairSettings> GetAsync(string assetPairId, string crossAssetPairId);

        Task AddAsync(CrossAssetPairSettings crossAssetPairSettings, string userId);

        Task UpdateAsync(CrossAssetPairSettings crossAssetPairSettings, string userId);

        Task DeleteAsync(string assetPairId, string crossAssetPairId, string userId);
    }
}
