using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settings
{
    public class TimersSettingsRepository : ITimersSettingsRepository
    {
        private readonly INoSQLTableStorage<TimersSettingsEntity> _storage;

        public TimersSettingsRepository(INoSQLTableStorage<TimersSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<TimersSettings> GetAsync()
        {
            TimersSettingsEntity entity = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey());

            return Mapper.Map<TimersSettings>(entity);
        }

        public async Task InsertOrReplaceAsync(TimersSettings timersSettings)
        {
            var entity = new TimersSettingsEntity(GetPartitionKey(), GetRowKey());

            Mapper.Map(timersSettings, entity);

            await _storage.InsertOrReplaceAsync(entity);
        }

        private static string GetPartitionKey()
            => "Timers";

        private static string GetRowKey()
            => "Timers";
    }
}
