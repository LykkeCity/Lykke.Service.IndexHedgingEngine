using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Simulation
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class SimulationParametersEntity : AzureTableEntity
    {
        private decimal _openTokens;
        private decimal _investments;

        public SimulationParametersEntity()
        {
        }

        public SimulationParametersEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string IndexName { get; set; }

        public decimal OpenTokens
        {
            get => _openTokens;
            set
            {
                if (_openTokens != value)
                {
                    _openTokens = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal Investments
        {
            get => _investments;
            set
            {
                if (_investments != value)
                {
                    _investments = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        [JsonValueSerializer] public IReadOnlyCollection<string> Assets { get; set; }
    }
}
