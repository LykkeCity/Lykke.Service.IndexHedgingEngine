using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IRiskExposureReportService
    {
        Task<RiskExposureReport> GetAsync();
    }
}
