namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents asset pair setting for exchange.
    /// </summary>
    public class AssetPairSettings
    {
        /// <summary>
        /// The name of asset pair.
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// The name of base asset.
        /// </summary>
        public string BaseAsset { get; set; }

        /// <summary>
        /// The name of quote asset.
        /// </summary>
        public string QuoteAsset { get; set; }

        /// <summary>
        /// The name of exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The identifier of the asset pair used for exchange.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The price accuracy of the limit order.
        /// </summary>
        public int PriceAccuracy { get; set; }

        /// <summary>
        /// The volume accuracy of the limit order.
        /// </summary>
        public int VolumeAccuracy { get; set; }

        /// <summary>
        /// The minimal volume of the limit order.
        /// </summary>
        public decimal MinVolume { get; set; }
    }
}
