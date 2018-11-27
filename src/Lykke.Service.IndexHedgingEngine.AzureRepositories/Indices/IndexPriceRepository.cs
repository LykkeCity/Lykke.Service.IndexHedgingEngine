using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Indices
{
    public class IndexPriceRepository : IIndexPriceRepository
    {
        private readonly INoSQLTableStorage<IndexPriceEntity> _storage;

        public IndexPriceRepository(INoSQLTableStorage<IndexPriceEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<IndexPrice>> GetAllAsync()
        {
            IList<IndexPriceEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<IndexPrice[]>(entities);
        }

        public async Task<IndexPrice> GetByIndexAsync(string indexName)
        {
            IndexPriceEntity entity = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey(indexName));

            return Mapper.Map<IndexPrice>(entity);
        }

        public async Task InsertAsync(IndexPrice indexPrice)
        {
            var entity = new IndexPriceEntity(GetPartitionKey(), GetRowKey(indexPrice.Name));

            Mapper.Map(indexPrice, entity);

            await _storage.InsertAsync(entity);
        }

        public async Task UpdateAsync(IndexPrice indexPrice)
        {
            await _storage.MergeAsync(GetPartitionKey(), GetRowKey(indexPrice.Name), entity =>
            {
                Mapper.Map(indexPrice, entity);
                return entity;
            });
        }

        public Task DeleteAsync(string indexName)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(indexName));
        }

        private static string GetPartitionKey()
            => "IndexPrice";

        private static string GetRowKey(string indexName)
            => indexName.ToUpper();
    }
}
