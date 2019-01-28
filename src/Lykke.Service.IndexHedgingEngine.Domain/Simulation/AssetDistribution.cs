namespace Lykke.Service.IndexHedgingEngine.Domain.Simulation
{
    /// <summary>
    /// Represent the part of investment for the asset.
    /// </summary>
    public class AssetDistribution
    {
        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// The weight of asset in the index.
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// The current asset price in USD.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The distributed amount of the asset.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The distributed amount in USD of the asset.
        /// </summary>
        public decimal AmountInUsd { get; set; }

        /// <summary>
        /// Indicates that the asset is hedged.
        /// </summary>
        public bool IsHedged { get; set; }
    }
}
