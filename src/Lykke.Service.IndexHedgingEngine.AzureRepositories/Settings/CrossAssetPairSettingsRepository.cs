using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

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
                GetRowKey(entity.Exchange, entity.AssetPairId));

            Mapper.Map(entity, newEntity);

            await _storage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(CrossAssetPairSettings entity)
        {
            return _storage.MergeAsync(
                GetPartitionKey(entity.IndexAssetPairId),
                GetRowKey(entity.Exchange, entity.AssetPairId),
                x =>
                {
                    Mapper.Map(entity, x);
                    return x;
                });
        }

        public Task DeleteAsync(string indexAssetPairId, string exchange, string assetPairId)
        {
            return _storage.DeleteAsync(
                GetPartitionKey(indexAssetPairId),
                GetRowKey(exchange, assetPairId));
        }

        private static string GetPartitionKey(string indexAssetPairId)
            => indexAssetPairId;

        private static string GetRowKey(string exchange, string assetPairId)
            => $"{exchange}_{assetPairId}";
    }
}
