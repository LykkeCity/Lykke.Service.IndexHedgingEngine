using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface ICrossAssetPairSettingsRepository
    {
        Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAllAsync();

        Task InsertAsync(CrossAssetPairSettings crossAssetPairSettings);
        
        Task UpdateAsync(CrossAssetPairSettings crossAssetPairSettings);
        
        Task DeleteAsync(Guid id);
    }
}
