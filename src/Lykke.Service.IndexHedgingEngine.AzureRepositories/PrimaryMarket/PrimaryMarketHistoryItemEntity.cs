using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.PrimaryMarket
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class PrimaryMarketHistoryItemEntity : AzureTableEntity
    {
        public PrimaryMarketHistoryItemEntity()
        {
        }

        public PrimaryMarketHistoryItemEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public DateTime DateTime { set; get; }
        
        public string AssetId { set; get; }
        
        public decimal Amount { set; get; }
        
        public string UserId { set; get; }
        
        public string Comment { set; get; }
    }
}
