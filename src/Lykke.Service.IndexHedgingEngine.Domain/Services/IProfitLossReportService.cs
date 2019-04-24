using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Reports;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IProfitLossReportService
    {
        Task<ProfitLossReport> GetAsync();
    }
}
