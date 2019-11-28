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
        private CrossAssetPairSettingsMode _mode;

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

        /// <summary>
        /// Base asset identifier.
        /// </summary>
        public string BaseAssetId { get; set; }

        /// <summary>
        /// Quote asset identifier.
        /// </summary>
        public string QuoteAssetId { get; set; }

        public string AssetPairId { get; set; }

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

        public CrossAssetPairSettingsMode Mode
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
