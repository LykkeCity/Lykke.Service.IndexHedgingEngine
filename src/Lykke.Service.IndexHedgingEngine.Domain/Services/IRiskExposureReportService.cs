using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Reports;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IRiskExposureReportService
    {
        Task<RiskExposureReport> GetAsync();
    }
}
