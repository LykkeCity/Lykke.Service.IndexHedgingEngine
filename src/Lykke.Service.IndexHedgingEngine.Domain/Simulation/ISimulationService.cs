using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Simulation
{
    public interface ISimulationService
    {
        Task<SimulationReport> GetReportAsync(string indexName);

        Task UpdateParametersAsync(string indexName, decimal openTokens, decimal investments);

        Task AddAssetAsync(string indexName, string asset);

        Task RemoveAssetAsync(string indexName, string asset);
    }
}
