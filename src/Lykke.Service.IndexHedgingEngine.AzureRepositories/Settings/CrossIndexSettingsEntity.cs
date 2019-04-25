using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class CrossIndexSettingsEntity : AzureTableEntity
    {
        public CrossIndexSettingsEntity()
        {
        }

        public CrossIndexSettingsEntity(string partitionKey, string rowKey)
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

        public string OriginalAssetPairId { get; set; }

        public string Exchange { get; set; }

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

        public string AssetPairId { get; set; }

        public string AssetId { get; set; }

        public string QuoteAssetId { get; set; }
    }
}
