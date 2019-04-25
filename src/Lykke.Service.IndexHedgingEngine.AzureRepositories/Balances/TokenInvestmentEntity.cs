using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Balances
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class TokenInvestmentEntity : AzureTableEntity
    {
        public TokenInvestmentEntity()
        {
        }

        public TokenInvestmentEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string AssetPairId { get; set; }

        public string AssetId { get; set; }

        public string QuoteAssetId { get; set; }

        private decimal _quoteVolume;
        public decimal QuoteVolume
        {
            get => _quoteVolume;
            set
            {
                if (_quoteVolume != value)
                {
                    _quoteVolume = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
    }
}
