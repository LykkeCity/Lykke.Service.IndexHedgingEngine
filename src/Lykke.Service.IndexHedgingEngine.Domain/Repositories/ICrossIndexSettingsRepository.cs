using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface ICrossIndexSettingsRepository
    {
        Task<IReadOnlyList<CrossIndexSettings>> GetAllAsync();

        Task InsertAsync(CrossIndexSettings entity);

        Task UpdateAsync(CrossIndexSettings entity);

        Task DeleteAsync(Guid id);
    }
}
