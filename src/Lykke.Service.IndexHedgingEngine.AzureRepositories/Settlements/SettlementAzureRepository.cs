using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Settlements;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settlements
{
    public class SettlementAzureRepository
    {
        private readonly INoSQLTableStorage<SettlementEntity> _storage;

        public SettlementAzureRepository(INoSQLTableStorage<SettlementEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<Settlement>> GetAllAsync()
        {
            IList<SettlementEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<Settlement[]>(entities);
        }

        public async Task<Settlement> GetByIdAsync(string settlementId)
        {
            SettlementEntity entity =
                await _storage.GetDataAsync(GetPartitionKey(settlementId), GetRowKey(settlementId));

            return Mapper.Map<Settlement>(entity);
        }

        public async Task InsertAsync(Settlement settlement)
        {
            var entity = new SettlementEntity(GetPartitionKey(settlement.Id), GetRowKey(settlement.Id));

            Mapper.Map(settlement, entity);

            await _storage.InsertAsync(entity);
        }

        public Task UpdateAsync(Settlement settlement)
        {
            return _storage.MergeAsync(GetPartitionKey(settlement.Id), GetRowKey(settlement.Id), entity =>
            {
                Mapper.Map(settlement, entity);
                return entity;
            });
        }
        
        public Task UpdateAsync(string settlementId, SettlementStatus status)
        {
            return _storage.MergeAsync(GetPartitionKey(settlementId), GetRowKey(settlementId), entity =>
            {
                entity.Status = status;
                return entity;
            });
        }
        
        private static string GetPartitionKey(string settlementId)
            => settlementId.CalculateHexHash32(2);

        private static string GetRowKey(string settlementId)
            => settlementId.ToUpper();
    }
}
