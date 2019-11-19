using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settings
{
    public class CrossAssetPairSettingsRepository : ICrossAssetPairSettingsRepository
    {
        private readonly INoSQLTableStorage<CrossAssetPairSettingsEntity> _storage;

        public CrossAssetPairSettingsRepository(INoSQLTableStorage<CrossAssetPairSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAllAsync()
        {
            IList<CrossAssetPairSettingsEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<CrossAssetPairSettings[]>(entities);
        }

        public async Task InsertAsync(CrossAssetPairSettings crossAssetPairSettings)
        {
            var entity = new CrossAssetPairSettingsEntity(GetPartitionKey(),
                GetRowKey(crossAssetPairSettings.Id));

            Mapper.Map(crossAssetPairSettings, entity);

            await _storage.InsertAsync(entity);
        }

        public Task UpdateAsync(CrossAssetPairSettings crossAssetPairSettings)
        {
            return _storage.MergeAsync(GetPartitionKey(), GetRowKey(crossAssetPairSettings.Id),
                entity =>
                {
                    Mapper.Map(crossAssetPairSettings, entity);
                    return entity;
                });
        }

        public Task DeleteAsync(Guid id)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(id));
        }

        private static string GetPartitionKey()
            => "Cross Asset Pair";

        private static string GetRowKey(Guid id)
            => id.ToString();
    }
}
