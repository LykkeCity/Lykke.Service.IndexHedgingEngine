using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IBalanceService
    {
        Task<IReadOnlyCollection<Balance>> GetAsync(string exchange);

        Task<Balance> GetByAssetIdAsync(string exchange, string assetId);

        Task UpdateAsync();
    }
}
