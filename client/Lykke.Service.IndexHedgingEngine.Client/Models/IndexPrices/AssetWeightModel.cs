using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.IndexPrices
{
    /// <summary>
    /// Represents an asset weight in index.
    /// </summary>
    [PublicAPI]
    public class AssetWeightModel
    {
        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The weight of asset in the index.
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// The middle price of the asset.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Indicates that the asset is disabled in index.
        /// </summary>
        public bool IsDisabled { get; set; }
    }
}
