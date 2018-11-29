using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.AzureStorage.Tables;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Balances
{
    public class BalanceOperationRepository : IBalanceOperationRepository
    {
        private readonly INoSQLTableStorage<BalanceOperationEntity> _storage;

        public BalanceOperationRepository(INoSQLTableStorage<BalanceOperationEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<BalanceOperation>> GetAsync(DateTime startDate, DateTime endDate,
            int limit, string assetId, BalanceOperationType balanceOperationType)
        {
            string filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.GreaterThan,
                    GetPartitionKey(endDate.Date.AddDays(1))),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(AzureTableEntity.PartitionKey), QueryComparisons.LessThan,
                    GetPartitionKey(startDate.Date.AddMilliseconds(-1))));

            if (!string.IsNullOrEmpty(assetId))
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(BalanceOperationEntity.AssetId),
                        QueryComparisons.Equal, assetId));
            }

            if (balanceOperationType != BalanceOperationType.None)
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(BalanceOperationEntity.Type),
                        QueryComparisons.Equal, balanceOperationType.ToString()));
            }

            var query = new TableQuery<BalanceOperationEntity>().Where(filter).Take(limit);

            IEnumerable<BalanceOperationEntity> entities = await _storage.WhereAsync(query);

            return Mapper.Map<List<BalanceOperation>>(entities);
        }

        public async Task InsertAsync(BalanceOperation balanceOperation)
        {
            var entity = new BalanceOperationEntity(GetPartitionKey(balanceOperation.Timestamp),
                GetRowKey(balanceOperation.Timestamp));

            Mapper.Map(balanceOperation, entity);

            await _storage.InsertAsync(entity);
        }

        private static string GetPartitionKey(DateTime timestamp)
            => (DateTime.MaxValue.Ticks - timestamp.Date.Ticks).ToString("D19");

        private static string GetRowKey(DateTime timestamp)
            => timestamp.ToString("O");
    }
}
