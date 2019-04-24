namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents token investment in index quote asset.
    /// </summary>
    public class TokenInvestment
    {
        /// <summary>
        /// Index asset pair identifier.
        /// </summary>
        public string IndexAssetPairId { get; set; }

        /// <summary>
        /// Invested volume in index quote asset.
        /// </summary>
        public decimal QuoteVolume { get; set; }
    }
}
