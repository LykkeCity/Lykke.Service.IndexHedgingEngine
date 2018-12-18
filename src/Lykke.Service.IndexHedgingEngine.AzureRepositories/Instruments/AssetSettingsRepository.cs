using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Instruments
{
    public class AssetSettingsRepository : IAssetSettingsRepository
    {
        private readonly INoSQLTableStorage<AssetSettingsEntity> _storage;

        public AssetSettingsRepository(INoSQLTableStorage<AssetSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<AssetSettings>> GetAllAsync()
        {
            IList<AssetSettingsEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<AssetSettings[]>(entities);
        }

        public async Task InsertAsync(AssetSettings assetSettings)
        {
            var entity = new AssetSettingsEntity(GetPartitionKey(assetSettings.Exchange),
                GetRowKey(assetSettings.Asset));

            Mapper.Map(assetSettings, entity);

            await _storage.InsertAsync(entity);
        }

        public Task UpdateAsync(AssetSettings assetSettings)
        {
            return _storage.MergeAsync(GetPartitionKey(assetSettings.Exchange), GetRowKey(assetSettings.Asset),
                entity =>
                {
                    Mapper.Map(assetSettings, entity);
                    return entity;
                });
        }

        public Task DeleteAsync(string asset, string exchange)
        {
            return _storage.DeleteAsync(GetPartitionKey(exchange), GetRowKey(asset));
        }

        private static string GetPartitionKey(string exchange)
            => exchange.ToUpper();

        private static string GetRowKey(string asset)
            => asset.ToUpper();
    }
}
