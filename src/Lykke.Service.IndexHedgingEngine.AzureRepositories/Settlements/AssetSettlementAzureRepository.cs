using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settlements
{
    public class AssetSettlementAzureRepository
    {
        private readonly INoSQLTableStorage<AssetSettlementEntity> _storage;

        public AssetSettlementAzureRepository(INoSQLTableStorage<AssetSettlementEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<AssetSettlement>> GetAllAsync()
        {
            IList<AssetSettlementEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<AssetSettlement[]>(entities);
        }

        public async Task<IReadOnlyCollection<AssetSettlement>> GetBySettlementIdAsync(string settlementId)
        {
            IEnumerable<AssetSettlementEntity> entities = await _storage.GetDataAsync(GetPartitionKey(settlementId));

            return Mapper.Map<AssetSettlement[]>(entities);
        }

        public async Task InsertAsync(IEnumerable<AssetSettlement> assetSettlements)
        {
            var entities = new List<AssetSettlementEntity>();

            foreach (AssetSettlement assetSettlement in assetSettlements)
            {
                var entity = new AssetSettlementEntity(GetPartitionKey(assetSettlement.SettlementId),
                    GetRowKey(assetSettlement.AssetId));

                Mapper.Map(assetSettlement, entity);
                
                entities.Add(entity);
            }
            
            await _storage.InsertAsync(entities);
        }
        
        public async Task ReplaceAsync(string settlementId, IEnumerable<AssetSettlement> assetSettlements)
        {
            IEnumerable<AssetSettlementEntity> entities = await _storage.GetDataAsync(GetPartitionKey(settlementId));

            await _storage.DeleteAsync(entities);

            await InsertAsync(assetSettlements);
        }

        private static string GetPartitionKey(string settlementId)
            => settlementId.ToUpper();

        private static string GetRowKey(string assetId)
            => assetId.ToUpper();
    }
}
