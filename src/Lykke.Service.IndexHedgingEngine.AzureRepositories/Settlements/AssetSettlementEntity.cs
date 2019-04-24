using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Settlements;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settlements
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class AssetSettlementEntity : AzureTableEntity
    {
        private decimal _amount;
        private decimal _price;
        private decimal _fee;
        private decimal _weight;
        private bool _isDirect;
        private bool _isExternal;
        private AssetSettlementStatus _status;
        private decimal _actualAmount;
        private decimal _actualPrice;

        public AssetSettlementEntity()
        {
        }

        public AssetSettlementEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string SettlementId { get; set; }

        public string AssetId { get; set; }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
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

        public decimal Fee
        {
            get => _fee;
            set
            {
                if (_fee != value)
                {
                    _fee = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal Weight
        {
            get => _weight;
            set
            {
                if (_weight != value)
                {
                    _weight = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public bool IsDirect
        {
            get => _isDirect;
            set
            {
                if (_isDirect != value)
                {
                    _isDirect = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public bool IsExternal
        {
            get => _isExternal;
            set
            {
                if (_isExternal != value)
                {
                    _isExternal = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public AssetSettlementStatus Status
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

        public string Error { get; set; }

        public decimal ActualAmount
        {
            get => _actualAmount;
            set
            {
                if (_actualAmount != value)
                {
                    _actualAmount = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public decimal ActualPrice
        {
            get => _actualPrice;
            set
            {
                if (_actualPrice != value)
                {
                    _actualPrice = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
        
        public string TransactionId { get; set; }
    }
}
