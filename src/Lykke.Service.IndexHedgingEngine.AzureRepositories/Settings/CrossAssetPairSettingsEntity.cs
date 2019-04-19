using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class CrossAssetPairSettingsEntity : AzureTableEntity
    {
        public CrossAssetPairSettingsEntity()
        {
        }

        public CrossAssetPairSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string AssetPairId { get; set; }

        public string CrossAssetPairId { get; set; }

        private bool _isInverted;
        public bool IsInverted
        {
            get => _isInverted;
            set
            {
                if (_isInverted != value)
                {
                    _isInverted = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
    }
}
