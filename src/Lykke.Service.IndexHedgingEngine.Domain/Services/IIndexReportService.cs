using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Reports;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IIndexReportService
    {
        Task<IReadOnlyCollection<IndexReport>> GetAsync();
    }
}
