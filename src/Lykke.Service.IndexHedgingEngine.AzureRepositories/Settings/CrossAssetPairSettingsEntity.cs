using System;
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

        private Guid? _id;
        public Guid? Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public string IndexAssetPairId { get; set; }

        public string Exchange { get; set; }

        public string AssetPairId { get; set; }

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
