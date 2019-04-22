using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ICrossAssetPairSettingsService
    {
        Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAllAsync();

        Task<CrossAssetPairSettings> GetAsync(Guid id);

        Task<Guid> AddAsync(CrossAssetPairSettings entity, string userId);

        Task UpdateAsync(CrossAssetPairSettings entity, string userId);

        Task DeleteAsync(Guid id, string userId);
    }
}
