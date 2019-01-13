using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class TimersSettingsEntity : AzureTableEntity
    {
        private TimeSpan _lykkeBalances;
        private TimeSpan _externalBalances;
        private TimeSpan _settlements;

        public TimersSettingsEntity()
        {
        }

        public TimersSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public TimeSpan LykkeBalances
        {
            get => _lykkeBalances;
            set
            {
                if (_lykkeBalances != value)
                {
                    _lykkeBalances = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public TimeSpan ExternalBalances
        {
            get => _externalBalances;
            set
            {
                if (_externalBalances != value)
                {
                    _externalBalances = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public TimeSpan Settlements
        {
            get => _settlements;
            set
            {
                if (_settlements != value)
                {
                    _settlements = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
    }
}
