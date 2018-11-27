using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Positions
{
    public class PositionRepository : IPositionRepository
    {
        private readonly INoSQLTableStorage<PositionEntity> _storage;

        public PositionRepository(INoSQLTableStorage<PositionEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<Position>> GetAllAsync()
        {
            IList<PositionEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<Position[]>(entities);
        }

        public async Task<Position> GetByAssetIdAsync(string assetId, string exchange)
        {
            PositionEntity entity = await _storage.GetDataAsync(GetPartitionKey(exchange), GetRowKey(assetId));

            return Mapper.Map<Position>(entity);
        }

        public async Task InsertAsync(Position position)
        {
            var entity = new PositionEntity(GetPartitionKey(position.Exchange), GetRowKey(position.AssetId));

            Mapper.Map(position, entity);

            await _storage.InsertAsync(entity);
        }

        public async Task UpdateAsync(Position position)
        {
            await _storage.MergeAsync(GetPartitionKey(position.Exchange), GetRowKey(position.AssetId), entity =>
            {
                Mapper.Map(position, entity);
                return entity;
            });
        }

        public Task DeleteAsync(string assetId, string exchange)
        {
            return _storage.DeleteAsync(GetPartitionKey(exchange), GetRowKey(assetId));
        }

        private static string GetPartitionKey(string exchange)
            => exchange.ToUpper();

        private static string GetRowKey(string assetId)
            => assetId.ToUpper();
    }
}
