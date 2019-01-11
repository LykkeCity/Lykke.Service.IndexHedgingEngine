using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface ISettlementRepository
    {
        Task<IReadOnlyCollection<Settlement>> GetAllAsync();

        Task<IReadOnlyCollection<Settlement>> GetByClientIdAsync(string clientId);

        Task<Settlement> GetByIdAsync(string settlementId);

        Task InsertAsync(Settlement settlement);

        Task UpdateAsync(Settlement settlement);

        Task UpdateStatusAsync(string settlementId, SettlementStatus status);
    }
}
