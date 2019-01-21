using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IPrimaryMarketBalanceUpdatesRepository
    {
        Task CreateAsync(PrimaryMarketHistoryItem item);
        Task<IReadOnlyList<PrimaryMarketHistoryItem>> GetItemsAsync();
    }
}
