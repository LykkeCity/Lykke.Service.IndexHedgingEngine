using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Balances
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class BalanceOperationEntity : AzureTableEntity
    {
        private DateTime _time;
        private decimal _amount;
        private BalanceOperationType _type;
        private bool _isCredit;

        public BalanceOperationEntity()
        {
        }

        public BalanceOperationEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
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

        public string AssetId { get; set; }

        public BalanceOperationType Type
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

        public bool IsCredit
        {
            get => _isCredit;
            set
            {
                if (_isCredit != value)
                {
                    _isCredit = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

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

        public string Comment { get; set; }

        public string UserId { get; set; }

        public string TransactionId { get; set; }
    }
}
