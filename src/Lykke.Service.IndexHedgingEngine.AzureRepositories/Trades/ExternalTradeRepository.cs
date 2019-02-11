using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Trades
{
    public class ExternalTradeRepository : IExternalTradeRepository
    {
        private readonly INoSQLTableStorage<ExternalTradeEntity> _storage;

        public ExternalTradeRepository(INoSQLTableStorage<ExternalTradeEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<ExternalTrade>> GetAsync(DateTime startDate, DateTime endDate,
            string exchange, string assetPairId, int limit)
        {
            string filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.GreaterThan,
                    GetPartitionKey(endDate.Date.AddDays(1))),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.LessThan,
                    GetPartitionKey(startDate.Date.AddMilliseconds(-1))));

            if (!string.IsNullOrEmpty(exchange))
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(ExternalTradeEntity.Exchange),
                        QueryComparisons.Equal, exchange));
            }

            if (!string.IsNullOrEmpty(assetPairId))
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(ExternalTradeEntity.AssetPairId),
                        QueryComparisons.Equal, assetPairId));
            }

            var query = new TableQuery<ExternalTradeEntity>().Where(filter).Take(limit);

            IEnumerable<ExternalTradeEntity> entities = await _storage.WhereAsync(query);

            return Mapper.Map<List<ExternalTrade>>(entities);
        }

        public async Task InsertAsync(ExternalTrade externalTrade)
        {
            var entity = new ExternalTradeEntity(GetPartitionKey(externalTrade.Timestamp), GetRowKey(externalTrade.Id));

            Mapper.Map(externalTrade, entity);

            await _storage.InsertOrReplaceAsync(entity);
        }

        private static string GetPartitionKey(DateTime timestamp)
            => (DateTime.MaxValue.Ticks - timestamp.Date.Ticks).ToString("D19");

        private static string GetRowKey(string externalTradeId)
            => externalTradeId;
    }
}
