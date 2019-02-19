using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IProfitLossReportService
    {
        Task<ProfitLossReport> GetAsync();
    }
}
