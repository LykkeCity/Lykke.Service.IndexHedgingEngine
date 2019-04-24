using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settings
{
    public class CrossIndexSettingsRepository : ICrossIndexSettingsRepository
    {
        private readonly INoSQLTableStorage<CrossIndexSettingsEntity> _storage;

        public CrossIndexSettingsRepository(INoSQLTableStorage<CrossIndexSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<CrossIndexSettings>> GetAllAsync()
        {
            IList<CrossIndexSettingsEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<CrossIndexSettings[]>(entities);
        }

        public async Task InsertAsync(CrossIndexSettings entity)
        {
            var newEntity = new CrossIndexSettingsEntity(
                GetPartitionKey(),
                GetRowKey(entity.Id));

            Mapper.Map(entity, newEntity);

            await _storage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(CrossIndexSettings entity)
        {
            return _storage.MergeAsync(
                GetPartitionKey(),
                GetRowKey(entity.Id),
                x =>
                {
                    Mapper.Map(entity, x);
                    return x;
                });
        }

        public Task DeleteAsync(Guid id)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(id));
        }

        private static string GetPartitionKey()
            => "CrossIndexSettings";

        private static string GetRowKey(Guid? id)
            => $"{id}";
    }
}
