using System;
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
        private decimal _marketOrderMarkup;
        private decimal _thresholdCritical;

        public HedgeSettingsEntity()
        {
        }

        public HedgeSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        [Obsolete]
        private decimal _thresholdUp;
        [Obsolete]
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

        [Obsolete]
        private decimal _thresholdDown;
        [Obsolete]
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

        private decimal _thresholdUpBuy;
        public decimal ThresholdUpBuy
        {
            get => _thresholdUpBuy;
            set
            {
                if (_thresholdUpBuy != value)
                {
                    _thresholdUpBuy = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        private decimal _thresholdUpSell;
        public decimal ThresholdUpSell
        {
            get => _thresholdUpSell;
            set
            {
                if (_thresholdUpSell != value)
                {
                    _thresholdUpSell = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        private decimal _thresholdDownBuy;
        public decimal ThresholdDownBuy
        {
            get => _thresholdDownBuy;
            set
            {
                if (_thresholdDownBuy != value)
                {
                    _thresholdDownBuy = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        private decimal _thresholdDownSell;
        public decimal ThresholdDownSell
        {
            get => _thresholdDownSell;
            set
            {
                if (_thresholdDownSell != value)
                {
                    _thresholdDownSell = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal ThresholdCritical
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
