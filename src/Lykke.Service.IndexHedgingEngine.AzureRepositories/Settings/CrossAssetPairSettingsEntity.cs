using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class CrossAssetPairSettingsEntity : AzureTableEntity
    {
        private Guid _id;
        private int _buySpread;
        private int _buyVolume;
        private int _sellSpread;
        private int _sellVolume;
        private CrossAssetPairsSettingsMode _mode;

        public CrossAssetPairSettingsEntity()
        {
        }

        public CrossAssetPairSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public Guid Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public string BaseAsset { get; set; }

        public string QuoteAsset { get; set; }

        public int BuySpread
        {
            get => _buySpread;
            set
            {
                if (_buySpread != value)
                {
                    _buySpread = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public int BuyVolume
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

        public int SellSpread
        {
            get => _sellSpread;
            set
            {
                if (_sellSpread != value)
                {
                    _sellSpread = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }

        public int SellVolume
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

        public CrossAssetPairsSettingsMode Mode
        {
            get => _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    MarkValueTypePropertyAsDirty();
                }
            }
        }
    }
}
