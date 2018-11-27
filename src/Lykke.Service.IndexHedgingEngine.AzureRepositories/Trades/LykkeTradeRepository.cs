using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Common;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Trades
{
    public class LykkeTradeRepository : ILykkeTradeRepository
    {
        private readonly INoSQLTableStorage<InternalTradeEntity> _storage;
        private readonly INoSQLTableStorage<AzureIndex> _indicesStorage;

        public LykkeTradeRepository(
            INoSQLTableStorage<InternalTradeEntity> storage,
            INoSQLTableStorage<AzureIndex> indicesStorage)
        {
            _storage = storage;
            _indicesStorage = indicesStorage;
        }

        public async Task<IReadOnlyCollection<InternalTrade>> GetAsync(DateTime startDate, DateTime endDate,
            string assetPairId, string oppositeWalletId, int limit)
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
                    TableQuery.GenerateFilterCondition(nameof(InternalTradeEntity.AssetPairId),
                        QueryComparisons.Equal, assetPairId));
            }

            if (!string.IsNullOrEmpty(oppositeWalletId))
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(InternalTradeEntity.OppositeWalletId),
                        QueryComparisons.Equal, oppositeWalletId));
            }

            var query = new TableQuery<InternalTradeEntity>().Where(filter).Take(limit);

            IEnumerable<InternalTradeEntity> entities = await _storage.WhereAsync(query);

            return Mapper.Map<List<InternalTrade>>(entities);
        }

        public async Task<InternalTrade> GetByIdAsync(string internalTradeId)
        {
            AzureIndex index = await _indicesStorage.GetDataAsync(GetIndexPartitionKey(internalTradeId),
                GetIndexRowKey(internalTradeId));

            if (index == null)
                return null;

            InternalTradeEntity entity = await _storage.GetDataAsync(index);

            return Mapper.Map<InternalTrade>(entity);
        }

        public async Task InsertAsync(InternalTrade internalTrade)
        {
            var entity = new InternalTradeEntity(GetPartitionKey(internalTrade.Timestamp), GetRowKey(internalTrade.Id));

            Mapper.Map(internalTrade, entity);

            await _storage.InsertAsync(entity);

            AzureIndex index = new AzureIndex(GetIndexPartitionKey(internalTrade.Id), GetRowKey(internalTrade.Id),
                entity);

            await _indicesStorage.InsertAsync(index);
        }

        private static string GetPartitionKey(DateTime timestamp)
            => (DateTime.MaxValue.Ticks - timestamp.Date.Ticks).ToString("D19");

        private static string GetRowKey(string internalTradeId)
            => internalTradeId;

        private static string GetIndexPartitionKey(string internalTradeId)
            => internalTradeId.CalculateHexHash32(4);

        private static string GetIndexRowKey(string internalTradeId)
            => internalTradeId;
    }
}
