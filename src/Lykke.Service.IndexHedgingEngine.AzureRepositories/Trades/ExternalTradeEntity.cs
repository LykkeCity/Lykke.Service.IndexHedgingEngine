using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Trades
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class ExternalTradeEntity : AzureTableEntity
    {
        private TradeType _type;
        private DateTime _date;
        private decimal _price;
        private decimal _volume;
        private TradeStatus _status;
        private decimal _originalVolume;
        private decimal _remainingVolume;

        public ExternalTradeEntity()
        {
        }

        public ExternalTradeEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Id { get; set; }

        public string Exchange { get; set; }

        public string LimitOrderId { get; set; }

        public string ExchangeOrderId { get; set; }

        public string AssetPairId { get; set; }

        public TradeType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
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

        public decimal Volume
        {
            get => _volume;
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public TradeStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal OriginalVolume
        {
            get => _originalVolume;
            set
            {
                if (_originalVolume != value)
                {
                    _originalVolume = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal RemainingVolume
        {
            get => _remainingVolume;
            set
            {
                if (_remainingVolume != value)
                {
                    _remainingVolume = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
    }
}
