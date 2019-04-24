using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Trades
{
    public class VirtualTradeRepository : IVirtualTradeRepository
    {
        private readonly INoSQLTableStorage<VirtualTradeEntity> _storage;

        public VirtualTradeRepository(INoSQLTableStorage<VirtualTradeEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<VirtualTrade>> GetAsync(DateTime startDate, DateTime endDate,
            string assetPairId, int limit)
        {
            string filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.GreaterThan,
                    GetPartitionKey(endDate.Date.AddDays(1))),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.LessThan,
                    GetPartitionKey(startDate.Date.AddMilliseconds(-1))));

            if (!string.IsNullOrEmpty(assetPairId))
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(VirtualTradeEntity.AssetPairId),
                        QueryComparisons.Equal, assetPairId));
            }

            var query = new TableQuery<VirtualTradeEntity>().Where(filter).Take(limit);

            IEnumerable<VirtualTradeEntity> entities = await _storage.WhereAsync(query);

            return Mapper.Map<VirtualTrade[]>(entities);
        }

        public async Task InsertAsync(VirtualTrade virtualTrade)
        {
            var entity = new VirtualTradeEntity(GetPartitionKey(virtualTrade.Timestamp), GetRowKey(virtualTrade.Id));

            Mapper.Map(virtualTrade, entity);

            await _storage.InsertAsync(entity);
        }

        private static string GetPartitionKey(DateTime time)
            => (DateTime.MaxValue.Ticks - time.Date.Ticks).ToString("D19");

        private static string GetRowKey(string virtualTradeId)
            => virtualTradeId;
    }
}
