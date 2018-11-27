using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Positions
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class PositionEntity : AzureTableEntity
    {
        private decimal _volume;
        private decimal _oppositeVolume;

        public PositionEntity()
        {
        }

        public PositionEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string AssetId { get; set; }
        
        public string Exchange { get; set; }

        public decimal Volume
        {
            get => _volume;
            set
            {
                if (_volume != value)
                {
                    _volume = value;
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
