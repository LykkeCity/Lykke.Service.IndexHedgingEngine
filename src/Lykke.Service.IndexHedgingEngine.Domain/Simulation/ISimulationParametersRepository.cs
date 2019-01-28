using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Simulation
{
    public interface ISimulationParametersRepository
    {
        Task<SimulationParameters> GetByIndexNameAsync(string indexName);
        
        Task SaveAsync(SimulationParameters simulationParameters);
    }
}
