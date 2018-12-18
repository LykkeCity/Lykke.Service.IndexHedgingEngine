using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Instruments
{
    public class AssetPairSettingsRepository : IAssetPairSettingsRepository
    {
        private readonly INoSQLTableStorage<AssetPairSettingsEntity> _storage;

        public AssetPairSettingsRepository(INoSQLTableStorage<AssetPairSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<AssetPairSettings>> GetAllAsync()
        {
            IList<AssetPairSettingsEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<AssetPairSettings[]>(entities);
        }

        public async Task InsertAsync(AssetPairSettings assetPairSettings)
        {
            var entity = new AssetPairSettingsEntity(GetPartitionKey(assetPairSettings.Exchange),
                GetRowKey(assetPairSettings.AssetPair));

            Mapper.Map(assetPairSettings, entity);

            await _storage.InsertAsync(entity);
        }

        public Task UpdateAsync(AssetPairSettings assetPairSettings)
        {
            return _storage.MergeAsync(GetPartitionKey(assetPairSettings.Exchange),
                GetRowKey(assetPairSettings.AssetPair), entity =>
                {
                    Mapper.Map(assetPairSettings, entity);
                    return entity;
                });
        }

        public Task DeleteAsync(string assetPair, string exchange)
        {
            return _storage.DeleteAsync(GetPartitionKey(exchange), GetRowKey(assetPair));
        }

        private static string GetPartitionKey(string exchange)
            => exchange.ToUpper();

        private static string GetRowKey(string assetPair)
            => assetPair.ToUpper();
    }
}
