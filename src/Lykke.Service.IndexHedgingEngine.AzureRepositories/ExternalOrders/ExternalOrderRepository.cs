using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.ExternalOrders
{
    public class ExternalOrderRepository : IExternalOrderRepository
    {
        private readonly INoSQLTableStorage<ExternalOrderEntity> _storage;

        public ExternalOrderRepository(INoSQLTableStorage<ExternalOrderEntity> storage)
        {
            _storage = storage;
        }

        public async Task<ExternalOrder> GetAsync(string exchange, string asset)
        {
            ExternalOrderEntity entity = await _storage.GetDataAsync(GetPartitionKey(exchange), GetRowKey(asset));

            return Mapper.Map<ExternalOrder>(entity);
        }

        public async Task InsertAsync(ExternalOrder externalOrder)
        {
            var entity =
                new ExternalOrderEntity(GetPartitionKey(externalOrder.Exchange), GetRowKey(externalOrder.Asset));

            Mapper.Map(externalOrder, entity);

            await _storage.InsertAsync(entity);
        }

        public Task DeleteAsync(string exchange, string asset)
        {
            return _storage.DeleteAsync(GetPartitionKey(exchange), GetRowKey(asset));
        }

        private static string GetPartitionKey(string exchange)
            => exchange.ToUpper();

        private static string GetRowKey(string asset)
            => asset.ToUpper();
    }
}
