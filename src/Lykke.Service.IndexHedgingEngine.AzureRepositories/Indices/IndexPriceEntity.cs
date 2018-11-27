using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Indices
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class IndexPriceEntity : AzureTableEntity
    {
        private decimal _value;
        private decimal _price;
        private decimal _k;
        private decimal _r;
        private decimal _delta;
        private DateTime _time;

        public IndexPriceEntity()
        {
        }

        public IndexPriceEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Name { get; set; }

        public decimal Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal K
        {
            get => _k;
            set
            {
                if (_k != value)
                {
                    _k = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal R
        {
            get => _r;
            set
            {
                if (_r != value)
                {
                    _r = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal Delta
        {
            get => _delta;
            set
            {
                if (_delta != value)
                {
                    _delta = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public DateTime Time
        {
            get => _time;
            set
            {
                if (_time != value)
                {
                    _time = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        [JsonValueSerializer]
        public IReadOnlyCollection<AssetWeight> Weights { get; set; }
    }
}
