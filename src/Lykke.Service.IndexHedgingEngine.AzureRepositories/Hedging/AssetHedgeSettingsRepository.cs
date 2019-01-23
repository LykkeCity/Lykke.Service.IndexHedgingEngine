using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Hedging
{
    public class AssetHedgeSettingsRepository : IAssetHedgeSettingsRepository
    {
        private readonly INoSQLTableStorage<AssetHedgeSettingsEntity> _storage;

        public AssetHedgeSettingsRepository(INoSQLTableStorage<AssetHedgeSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<AssetHedgeSettings>> GetAllAsync()
        {
            IList<AssetHedgeSettingsEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<AssetHedgeSettings[]>(entities);
        }

        public async Task<AssetHedgeSettings> GetByAssetIdAsync(string assetId)
        {
            AssetHedgeSettingsEntity entity = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey(assetId));

            return Mapper.Map<AssetHedgeSettings>(entity);
        }

        public async Task InsertAsync(AssetHedgeSettings assetHedgeSettings)
        {
            var entity = new AssetHedgeSettingsEntity(GetPartitionKey(), GetRowKey(assetHedgeSettings.AssetId));

            Mapper.Map(assetHedgeSettings, entity);

            await _storage.InsertAsync(entity);
        }

        public async Task UpdateAsync(AssetHedgeSettings assetHedgeSettings)
        {
            await _storage.ReplaceAsync(GetPartitionKey(), GetRowKey(assetHedgeSettings.AssetId), entity =>
            {
                Mapper.Map(assetHedgeSettings, entity);
                return entity;
            });
        }

        public Task DeleteAsync(string assetId)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(assetId));
        }

        private static string GetPartitionKey()
            => "AssetHedgeSettings";

        private static string GetRowKey(string assetId)
            => assetId.ToUpper();
    }
}
