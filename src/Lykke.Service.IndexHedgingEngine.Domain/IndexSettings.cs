namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents an index settings
    /// </summary>
    public class IndexSettings
    {
        /// <summary>
        /// The name of the index.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The identifier of the asset that associated with index.
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
        }
    }
}
