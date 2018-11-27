using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IPositionReportService
    {
        Task<IReadOnlyCollection<PositionReport>> GetAsync();

        Task<IReadOnlyCollection<PositionReport>> GetByExchangeAsync(string exchange);
    }
}
