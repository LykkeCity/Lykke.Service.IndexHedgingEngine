using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settings
{
    [Obsolete]
    public class AssetLinkRepository : IAssetLinkRepository
    {
        private readonly INoSQLTableStorage<AssetLinkEntity> _storage;

        public AssetLinkRepository(INoSQLTableStorage<AssetLinkEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<AssetLink>> GetAllAsync()
        {
            IList<AssetLinkEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<AssetLink[]>(entities);
        }

        public Task InsertAsync(AssetLink assetLink)
        {
            var entity = new AssetLinkEntity(GetPartitionKey(), GetRowKey(assetLink.AssetId));

            Mapper.Map(assetLink, entity);

            return _storage.InsertAsync(entity);
        }

        public Task UpdateAsync(AssetLink assetLink)
        {
            return _storage.MergeAsync(GetPartitionKey(), GetRowKey(assetLink.AssetId), entity =>
            {
                Mapper.Map(assetLink, entity);
                return entity;
            });
        }

        public Task DeleteAsync(string assetId)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(assetId));
        }

        private static string GetPartitionKey()
            => "AssetLink";

        private static string GetRowKey(string assetId)
            => assetId.ToUpper();
    }
}
