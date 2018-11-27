using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Balances
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class TokenEntity : AzureTableEntity
    {
        private decimal _amount;
        private decimal _openVolume;
        private decimal _oppositeVolume;

        public TokenEntity()
        {
        }

        public TokenEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string AssetId { get; set; }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal OpenVolume
        {
            get => _openVolume;
            set
            {
                if (_openVolume != value)
                {
                    _openVolume = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal OppositeVolume
        {
            get => _oppositeVolume;
            set
            {
                if (_oppositeVolume != value)
                {
                    _oppositeVolume = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
    }
}
