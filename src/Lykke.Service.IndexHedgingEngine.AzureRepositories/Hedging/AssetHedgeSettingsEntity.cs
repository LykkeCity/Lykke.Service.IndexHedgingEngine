using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Hedging
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class AssetHedgeSettingsEntity : AzureTableEntity
    {
        private decimal _minVolume;
        private int _volumeAccuracy;
        private int _priceAccuracy;
        private bool _approved;
        private bool _enabled;

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

        public decimal MinVolume
        {
            get => _minVolume;
            set
            {
                if (_minVolume != value)
                {
                    _minVolume = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public int VolumeAccuracy
        {
            get => _volumeAccuracy;
            set
            {
                if (_volumeAccuracy != value)
                {
                    _volumeAccuracy = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public int PriceAccuracy
        {
            get => _priceAccuracy;
            set
            {
                if (_priceAccuracy != value)
                {
                    _priceAccuracy = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

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

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
    }
}
