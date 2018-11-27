using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.AssetLinks
{
    /// <summary>
    /// Represents a mapping between external and Lykke asset.
    /// </summary>
    [PublicAPI]
    public class AssetLinkModel
    {
        /// <summary>
        /// The identifier of asset in index.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The identifier of asset in Lykke exchange.
        /// </summary>
        public string LykkeAssetId { get; set; }
    }
}
