using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.HedgeLimitOrders
{
    public class HedgeLimitOrderRepository : IHedgeLimitOrderRepository
    {
        private readonly INoSQLTableStorage<HedgeLimitOrderEntity> _storage;

        public HedgeLimitOrderRepository(INoSQLTableStorage<HedgeLimitOrderEntity> storage)
        {
            _storage = storage;
        }

        public async Task<HedgeLimitOrder> GetByIdAsync(string hedgeLimitOrderId)
        {
            HedgeLimitOrderEntity entity =
                await _storage.GetDataAsync(GetPartitionKey(hedgeLimitOrderId), GetRowKey(hedgeLimitOrderId));

            return Mapper.Map<HedgeLimitOrder>(entity);
        }

        public Task InsertAsync(HedgeLimitOrder hedgeLimitOrder)
        {
            var entity = new HedgeLimitOrderEntity(GetPartitionKey(hedgeLimitOrder.Id), GetRowKey(hedgeLimitOrder.Id));

            Mapper.Map(hedgeLimitOrder, entity);

            return _storage.InsertAsync(entity);
        }

        public async Task InsertAsync(IEnumerable<HedgeLimitOrder> hedgeLimitOrders)
        {
            foreach (HedgeLimitOrder hedgeLimitOrder in hedgeLimitOrders)
            {
                var entity = new HedgeLimitOrderEntity(GetPartitionKey(hedgeLimitOrder.Id),
                    GetRowKey(hedgeLimitOrder.Id));

                Mapper.Map(hedgeLimitOrder, entity);

                await _storage.InsertAsync(entity);
            }
        }

        private static string GetPartitionKey(string hedgeLimitOrderId)
            => hedgeLimitOrderId.CalculateHexHash32(4);

        private static string GetRowKey(string hedgeLimitOrderId)
            => hedgeLimitOrderId;
    }
}
