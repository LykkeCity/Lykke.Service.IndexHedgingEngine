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
        /// <param name="price">The middle price of the asset.</param>
        /// <param name="isDisabled">Indicates that the asset is disabled in index.</param>
        public AssetWeight(string assetId, decimal weight, decimal price, bool isDisabled)
        {
            AssetId = assetId;
            Weight = weight;
            Price = price;
            IsDisabled = isDisabled;
        }

        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string AssetId { get; }

        /// <summary>
        /// The weight of asset in the index.
        /// </summary>
        public decimal Weight { get; }

        /// <summary>
        /// The middle price of the asset.
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// Indicates that the asset is disabled in index.
        /// </summary>
        public bool IsDisabled { get; }
    }
}
