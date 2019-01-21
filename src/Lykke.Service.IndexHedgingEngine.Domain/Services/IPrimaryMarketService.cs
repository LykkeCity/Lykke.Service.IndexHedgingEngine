using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IPrimaryMarketService
    {
        Task<string> GetPrimaryMarketWalletIdAsync();
        
        Task<IReadOnlyList<PrimaryMarketBalance>> GetBalancesAsync();
        
        Task UpdateBalanceAsync(string assetId, decimal amount, string userId, string comment);
        
        Task<IReadOnlyList<PrimaryMarketHistoryItem>> GetHistoryAsync();
    }
}
