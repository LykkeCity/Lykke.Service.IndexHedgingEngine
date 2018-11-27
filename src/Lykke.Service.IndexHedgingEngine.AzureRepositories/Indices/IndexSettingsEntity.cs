using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Indices
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class IndexSettingsEntity : AzureTableEntity
    {
        private decimal _alpha;
        private decimal _trackingFee;
        private decimal _performanceFee;
        private decimal _sellMarkup;
        private decimal _sellVolume;
        private decimal _buyVolume;

        public IndexSettingsEntity()
        {
        }

        public IndexSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Name { get; set; }

        public string AssetId { get; set; }

        public string AssetPairId { get; set; }

        public decimal Alpha
        {
            get => _alpha;
            set
            {
                if (_alpha != value)
                {
                    _alpha = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal TrackingFee
        {
            get => _trackingFee;
            set
            {
                if (_trackingFee != value)
                {
                    _trackingFee = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal PerformanceFee
        {
            get => _performanceFee;
            set
            {
                if (_performanceFee != value)
                {
                    _performanceFee = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal SellMarkup
        {
            get => _sellMarkup;
            set
            {
                if (_sellMarkup != value)
                {
                    _sellMarkup = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal SellVolume
        {
            get => _sellVolume;
            set
            {
                if (_sellVolume != value)
                {
                    _sellVolume = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal BuyVolume
        {
            get => _buyVolume;
            set
            {
                if (_buyVolume != value)
                {
                    _buyVolume = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
    }
}
