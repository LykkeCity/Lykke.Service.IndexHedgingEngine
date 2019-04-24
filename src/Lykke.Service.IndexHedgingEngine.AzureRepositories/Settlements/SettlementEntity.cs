using System;
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
    public class SettlementEntity : AzureTableEntity
    {
        private decimal _amount;
        private decimal _price;
        private bool _isDirect;
        private SettlementStatus _status;
        private DateTime _createdDate;

        public SettlementEntity()
        {
        }

        public SettlementEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Id { get; set; }

        public string IndexName { get; set; }

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

        public string WalletId { get; set; }

        public string ClientId { get; set; }

        public string Comment { get; set; }

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

        public SettlementStatus Status
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

        public string CreatedBy { get; set; }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                if (_createdDate != value)
                {
                    _createdDate = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
        
        public string TransactionId { get; set; }
    }
}
