using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ICrossAssetPairSettingsService
    {
        Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAllAsync();

        Task<CrossAssetPairSettings> GetAsync(string indexAssetPairId, string exchange, string assetPairId);

        Task AddAsync(CrossAssetPairSettings entity, string userId);

        Task UpdateAsync(CrossAssetPairSettings entity, string userId);

        Task DeleteAsync(string indexAssetPairId, string exchange, string assetPairId, string userId);
    }
}
