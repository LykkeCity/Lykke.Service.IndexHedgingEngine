namespace Lykke.Service.IndexHedgingEngine.Domain.Settings
{
    /// <summary>
    /// Represents an index settings
    /// </summary>
    public class IndexSettings
    {
        private int _sellLimitOrdersCount;
        private int _buyLimitOrdersCount;

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The identifier of the asset that associated with index (LyCI).
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The identifier of the asset pair that used to create limit orders.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The alpha coefficient.
        /// </summary>
        public decimal Alpha { get; set; }

        /// <summary>
        /// The tracking fee.
        /// </summary>
        public decimal TrackingFee { get; set; }

        /// <summary>
        /// The performance fee.
        /// </summary>
        public decimal PerformanceFee { get; set; }

        /// <summary>
        /// The sell limit order markup.
        /// </summary>
        public decimal SellMarkup { get; set; }

        /// <summary>
        /// The volume of sell limit order.
        /// </summary>
        public decimal SellVolume { get; set; }

        /// <summary>
        /// The volume of buy limit order.
        /// </summary>
        public decimal BuyVolume { get; set; }

        /// <summary>
        /// The number of sell limit orders.
        /// </summary>
        public int SellLimitOrdersCount
        {
            get => _sellLimitOrdersCount <= 0 ? 1 : _sellLimitOrdersCount;
            set => _sellLimitOrdersCount = value;
        }

        /// <summary>
        /// The number of buy limit orders.
        /// </summary>
        public int BuyLimitOrdersCount
        {
            get => _buyLimitOrdersCount <= 0 ? 1 : _buyLimitOrdersCount;
            set => _buyLimitOrdersCount = value;
        }

        public void Update(IndexSettings indexSettings)
        {
            AssetId = indexSettings.AssetId;
            AssetPairId = indexSettings.AssetPairId;
            Alpha = indexSettings.Alpha;
            TrackingFee = indexSettings.TrackingFee;
            PerformanceFee = indexSettings.PerformanceFee;
            SellMarkup = indexSettings.SellMarkup;
            SellVolume = indexSettings.SellVolume;
            BuyVolume = indexSettings.BuyVolume;
            SellLimitOrdersCount = indexSettings.SellLimitOrdersCount;
            BuyLimitOrdersCount = indexSettings.BuyLimitOrdersCount;
        }
    }
}
