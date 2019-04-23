using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ICrossIndexSettingsService
    {
        Task<IReadOnlyCollection<CrossIndexSettings>> GetAllAsync();

        Task<CrossIndexSettings> GetAsync(Guid id);

        Task<IReadOnlyList<CrossIndexSettings>> FindByIndexAssetPairAsync(string indexAssetPairId);

        Task<Guid> AddAsync(CrossIndexSettings entity, string userId);

        Task UpdateAsync(CrossIndexSettings entity, string userId);

        Task DeleteAsync(Guid id, string userId);
    }
}
