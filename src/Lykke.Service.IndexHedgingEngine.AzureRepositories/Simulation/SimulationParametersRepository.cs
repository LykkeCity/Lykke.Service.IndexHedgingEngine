using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain.Simulation;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Simulation
{
    public class SimulationParametersRepository : ISimulationParametersRepository
    {
        private readonly INoSQLTableStorage<SimulationParametersEntity> _storage;

        public SimulationParametersRepository(INoSQLTableStorage<SimulationParametersEntity> storage)
        {
            _storage = storage;
        }

        public async Task<SimulationParameters> GetByIndexNameAsync(string indexName)
        {
            SimulationParametersEntity entity = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey(indexName));

            return Mapper.Map<SimulationParameters>(entity);
        }

        public async Task SaveAsync(SimulationParameters simulationParameters)
        {
            var entity = new SimulationParametersEntity(GetPartitionKey(), GetRowKey(simulationParameters.IndexName));

            Mapper.Map(simulationParameters, entity);

            await _storage.InsertOrReplaceAsync(entity);
        }

        private static string GetPartitionKey()
            => "SimulationParameters";

        private static string GetRowKey(string indexName)
            => indexName.ToUpper();
    }
}
