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

        public async Task InsertAsync(CrossAssetPairSettings crossAssetPairSettings)
        {
            var entity = new CrossAssetPairSettingsEntity(
                GetPartitionKey(crossAssetPairSettings.AssetPairId),
                GetRowKey(crossAssetPairSettings.CrossAssetPairId));

            Mapper.Map(crossAssetPairSettings, entity);

            await _storage.InsertAsync(entity);
        }

        public Task UpdateAsync(CrossAssetPairSettings crossAssetPairSettings)
        {
            return _storage.MergeAsync(
                GetPartitionKey(crossAssetPairSettings.AssetPairId),
                GetRowKey(crossAssetPairSettings.CrossAssetPairId),
                entity =>
                {
                    Mapper.Map(crossAssetPairSettings, entity);
                    return entity;
                });
        }

        public Task DeleteAsync(string assetPairId, string crossAssetPairId)
        {
            return _storage.DeleteAsync(
                GetPartitionKey(assetPairId),
                GetRowKey(crossAssetPairId));
        }

        private static string GetPartitionKey(string assetPairId)
            => assetPairId;

        private static string GetRowKey(string crossAssetPairId)
            => crossAssetPairId;
    }
}
