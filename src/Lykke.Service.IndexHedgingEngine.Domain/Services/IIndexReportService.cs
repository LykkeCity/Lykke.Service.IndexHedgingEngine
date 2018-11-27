using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IIndexReportService
    {
        Task<IReadOnlyCollection<IndexReport>> GetAsync();
    }
}
