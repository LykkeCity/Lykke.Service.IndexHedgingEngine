using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settings
{
    public class HedgeSettingsRepository : IHedgeSettingsRepository
    {
        private readonly INoSQLTableStorage<HedgeSettingsEntity> _storage;

        public HedgeSettingsRepository(INoSQLTableStorage<HedgeSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<HedgeSettings> GetAsync()
        {
            HedgeSettingsEntity entity = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey());

            return Mapper.Map<HedgeSettings>(entity);
        }

        public async Task InsertOrReplaceAsync(HedgeSettings hedgeSettings)
        {
            var entity = new HedgeSettingsEntity(GetPartitionKey(), GetRowKey());

            Mapper.Map(hedgeSettings, entity);

            await _storage.InsertOrReplaceAsync(entity);
        }

        private static string GetPartitionKey()
            => "HedgeSettings";

        private static string GetRowKey()
            => "HedgeSettings";
    }
}
