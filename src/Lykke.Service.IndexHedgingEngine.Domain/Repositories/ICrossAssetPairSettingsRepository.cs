using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface ICrossAssetPairSettingsRepository
    {
        Task<IReadOnlyList<CrossAssetPairSettings>> GetAllAsync();

        Task InsertAsync(CrossAssetPairSettings crossAssetPairSettings);

        Task UpdateAsync(CrossAssetPairSettings crossAssetPairSettings);

        Task DeleteAsync(string assetPairId, string crossAssetPairId);
    }
}
