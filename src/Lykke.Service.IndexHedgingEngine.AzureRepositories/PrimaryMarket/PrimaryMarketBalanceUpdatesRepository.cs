using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.PrimaryMarket
{
    [UsedImplicitly]
    public class PrimaryMarketBalanceUpdatesRepository : IPrimaryMarketBalanceUpdatesRepository
    {
        private readonly INoSQLTableStorage<PrimaryMarketBalanceUpdateEntity> _storage;
        
        public PrimaryMarketBalanceUpdatesRepository(
            INoSQLTableStorage<PrimaryMarketBalanceUpdateEntity> storage)
        {
            _storage = storage;
        }
        
        public Task CreateAsync(PrimaryMarketHistoryItem item)
        {
            var entity = new PrimaryMarketBalanceUpdateEntity(GetPartitionKey(item.AssetId), GetRowKey(item.DateTime));

            Mapper.Map(item, entity);

            return _storage.InsertAsync(entity);
        }

        public async Task<IReadOnlyList<PrimaryMarketHistoryItem>> GetItemsAsync()
        {
            var data = await _storage.GetDataAsync();
            
            return Mapper.Map<PrimaryMarketHistoryItem[]>(data.OrderByDescending(x => x.DateTime));
        }
        
        private static string GetPartitionKey(string assetId)
            => assetId;

        private static string GetRowKey(DateTime dateTime)
            => DateTime
                .MaxValue
                .Subtract(dateTime)
                .TotalMilliseconds
                .ToString(CultureInfo.InvariantCulture);
    }
}
