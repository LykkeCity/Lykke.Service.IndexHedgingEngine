using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IHedgeLimitOrderRepository
    {
        Task<HedgeLimitOrder> GetByIdAsync(string hedgeLimitOrderId);

        Task InsertAsync(HedgeLimitOrder hedgeLimitOrder);

        Task InsertAsync(IEnumerable<HedgeLimitOrder> hedgeLimitOrders);
    }
}
