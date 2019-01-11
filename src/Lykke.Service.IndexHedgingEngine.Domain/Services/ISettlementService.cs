using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ISettlementService
    {
        Task<IReadOnlyCollection<Settlement>> GetAllAsync();

        Task<IReadOnlyCollection<Settlement>> GetByClientIdAsync(string clientId);

        Task<Settlement> GetByIdAsync(string settlementId);

        Task CreateAsync(string indexName, decimal amount, string comment, string walletId, string clientId,
            string userId, bool isDirect);

        Task RecalculateAsync(string settlementId, string userId);

        Task ApproveAsync(string settlementId, string userId);

        Task RejectAsync(string settlementId, string userId);
    }
}