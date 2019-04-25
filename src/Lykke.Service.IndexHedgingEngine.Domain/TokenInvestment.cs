namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents token investment in index quote asset.
    /// </summary>
    public class TokenInvestment
    {
        /// <summary>
        /// Index asset pair identifier (LyCI/USD, LyCI/EUR).
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// Asset identifier of the cross index (LyCI, P-LyCI).
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Quote asset identifier of the cross index (USD, EUR).
        /// </summary>
        public string QuoteAssetId { get; set; }

        /// <summary>
        /// Invested volume in index quote asset.
        /// </summary>
        public decimal QuoteVolume { get; set; }
    }
}
