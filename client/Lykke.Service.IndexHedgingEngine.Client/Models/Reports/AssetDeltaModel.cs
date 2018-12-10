using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Reports
{
    /// <summary>
    /// Represents a position delta value for an asset.
    /// </summary>
    [PublicAPI]
    public class AssetDeltaModel
    {
        /// <summary>
        /// The asset identifier.
        /// </summary>
        public string AssetId { get; set; }
        
        /// <summary>
        /// The position delta value.
        /// </summary>
        public decimal Delta { get; set; }
    }
}
