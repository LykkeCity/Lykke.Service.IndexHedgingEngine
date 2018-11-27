using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Indices
{
    public class IndexSettingsRepository : IIndexSettingsRepository
    {
        private readonly INoSQLTableStorage<IndexSettingsEntity> _storage;

        public IndexSettingsRepository(INoSQLTableStorage<IndexSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<IndexSettings>> GetAllAsync()
        {
            IList<IndexSettingsEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<IndexSettings[]>(entities);
        }

        public async Task<IndexSettings> GetByIndexAsync(string indexName)
        {
            IndexSettingsEntity entity = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey(indexName));

            return Mapper.Map<IndexSettings>(entity);
        }

        public async Task InsertAsync(IndexSettings indexSettings)
        {
            var entity = new IndexSettingsEntity(GetPartitionKey(), GetRowKey(indexSettings.Name));

            Mapper.Map(indexSettings, entity);

            await _storage.InsertAsync(entity);
        }

        public async Task UpdateAsync(IndexSettings indexSettings)
        {
            await _storage.MergeAsync(GetPartitionKey(), GetRowKey(indexSettings.Name), entity =>
            {
                Mapper.Map(indexSettings, entity);
                return entity;
            });
        }

        public Task DeleteAsync(string indexName)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(indexName));
        }

        private static string GetPartitionKey()
            => "IndexSettings";

        private static string GetRowKey(string indexName)
            => indexName.ToUpper();
    }
}
