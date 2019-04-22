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
    public class CrossAssetPairSettingsRepository : ICrossAssetPairSettingsRepository
    {
        private readonly INoSQLTableStorage<CrossAssetPairSettingsEntity> _storage;

        public CrossAssetPairSettingsRepository(INoSQLTableStorage<CrossAssetPairSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<CrossAssetPairSettings>> GetAllAsync()
        {
            IList<CrossAssetPairSettingsEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<CrossAssetPairSettings[]>(entities);
        }

        public async Task InsertAsync(CrossAssetPairSettings entity)
        {
            var newEntity = new CrossAssetPairSettingsEntity(
                GetPartitionKey(entity.IndexAssetPairId),
                GetRowKey(entity.Id));

            Mapper.Map(entity, newEntity);

            await _storage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(CrossAssetPairSettings entity)
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

            var query = new TableQuery<CrossAssetPairSettingsEntity>().Where(filter);

            var model = (await _storage.WhereAsync(query)).Single();

            await _storage.DeleteAsync(model);
        }

        private static string GetPartitionKey(string indexAssetPairId)
            => indexAssetPairId;

        private static string GetRowKey(Guid? id)
            => $"{id}";
    }
}
