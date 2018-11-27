using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class HedgeSettingsEntity : AzureTableEntity
    {
        private decimal _thresholdUp;
        private decimal _thresholdDown;
        private decimal _marketOrderMarkup;

        public HedgeSettingsEntity()
        {
        }

        public HedgeSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public decimal ThresholdUp
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

        public decimal ThresholdDown
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

        public decimal MarketOrderMarkup
        {
            get => _marketOrderMarkup;
            set
            {
                if (_marketOrderMarkup != value)
                {
                    _marketOrderMarkup = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
    }
}
