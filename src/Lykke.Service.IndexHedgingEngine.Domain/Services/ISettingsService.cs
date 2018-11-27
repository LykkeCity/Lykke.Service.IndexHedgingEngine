using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ISettingsService
    {
        Task<string> GetInstanceNameAsync();

        Task<string> GetWalletIdAsync();

        Task<IReadOnlyCollection<ExchangeSettings>> GetExchangesAsync();
    }
}
