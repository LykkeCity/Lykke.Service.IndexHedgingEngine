using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.AzureStorage.Tables;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;
using Microsoft.WindowsAzure.Storage.Table;

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
                GetPartitionKey(entity.IndexAssetPairId),
                GetRowKey(entity.Id));

            Mapper.Map(entity, newEntity);

            await _storage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(CrossIndexSettings entity)
        {
            return _storage.MergeAsync(
                GetPartitionKey(entity.IndexAssetPairId),
                GetRowKey(entity.Id),
                x =>
                {
                    Mapper.Map(entity, x);
                    return x;
                });
        }

        public async Task DeleteAsync(Guid id)
        {
            string filter = TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.RowKey), QueryComparisons.Equal, GetRowKey(id));

            var query = new TableQuery<CrossIndexSettingsEntity>().Where(filter);

            var model = (await _storage.WhereAsync(query)).Single();

            await _storage.DeleteAsync(model);
        }

        private static string GetPartitionKey(string indexAssetPairId)
            => indexAssetPairId;

        private static string GetRowKey(Guid? id)
            => $"{id}";
    }
}
