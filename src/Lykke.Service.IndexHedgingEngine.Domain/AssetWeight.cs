namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents an asset weight in the index.
    /// </summary>
    public class AssetWeight
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AssetWeight"/>.
        /// </summary>
        /// <param name="assetId">The identifier of the asset.</param>
        /// <param name="weight">The weight of asset in the index.</param>
        public AssetWeight(string assetId, decimal weight)
        {
            AssetId = assetId;
            Weight = weight;
        }

        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string AssetId { get; }

        /// <summary>
        /// The weight of asset in the index.
        /// </summary>
        public decimal Weight { get; }
    }
}
