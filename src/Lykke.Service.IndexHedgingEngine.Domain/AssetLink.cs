using System;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents a mapping between external and Lykke asset.
    /// </summary>
    [Obsolete]
    public class AssetLink
    {
        /// <summary>
        /// The identifier of asset in index.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The identifier of asset in Lykke exchange.
        /// </summary>
        public string LykkeAssetId { get; set; }

        public void Update(AssetLink assetLink)
        {
            LykkeAssetId = assetLink.LykkeAssetId;
        }
    }
}
