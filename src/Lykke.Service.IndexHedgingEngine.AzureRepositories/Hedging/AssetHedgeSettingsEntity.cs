using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Hedging
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class AssetHedgeSettingsEntity : AzureTableEntity
    {
        private bool _approved;
        private AssetHedgeMode _mode;
        private decimal? _referenceDelta;
        private decimal? _thresholdUp;
        private decimal? _thresholdDown;
        private decimal? _thresholdCritical;

        public AssetHedgeSettingsEntity()
        {
        }

        public AssetHedgeSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string AssetId { get; set; }

        public string Exchange { get; set; }

        public string AssetPairId { get; set; }

        public bool Approved
        {
            get => _approved;
            set
            {
                if (_approved != value)
                {
                    _approved = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public AssetHedgeMode Mode
        {
            get => _mode == AssetHedgeMode.None ? AssetHedgeMode.Disabled : _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public string ReferenceExchange { get; set; }

        public decimal? ReferenceDelta
        {
            get => _referenceDelta;
            set
            {
                if (_referenceDelta != value)
                {
                    _referenceDelta = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal? ThresholdUp
        {
            get => _thresholdUp;
            set
            {
                if (_thresholdUp != value)
                {
                    _thresholdUp = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal? ThresholdDown
        {
            get => _thresholdDown;
            set
            {
                if (_thresholdDown != value)
                {
                    _thresholdDown = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal? ThresholdCritical
        {
            get => _thresholdCritical;
            set
            {
                if (_thresholdCritical != value)
                {
                    _thresholdCritical = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
    }
}
