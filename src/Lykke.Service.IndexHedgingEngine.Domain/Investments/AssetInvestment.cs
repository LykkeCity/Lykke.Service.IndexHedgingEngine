using System.Collections.Generic;

namespace Lykke.Service.IndexHedgingEngine.Domain.Investments
{
    /// <summary>
    /// Represents an asset investment details.
    /// </summary>
    public class AssetInvestment
    {
        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string AssetId { get; set; }
        
        /// <summary>
        /// The current open position volume.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// The current asset price in USD.
        /// </summary>
        public Quote Quote { get; set; }

        /// <summary>
        /// The distributed amount of the asset.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// The remaining volume of current position (<see cref="Volume"/> * <see cref="Quote"/> - <see cref="TotalAmount"/>).
        /// </summary>
        public decimal RemainingAmount { get; set; }
        
        /// <summary>
        /// Indicates that the asset is disabled in index.
        /// </summary>
        public bool IsDisabled { get; set; }
        
        /// <summary>
        /// A collection of asset investment in indices.
        /// </summary>
        public IReadOnlyCollection<AssetIndexInvestment> Indices { get; set; }
    }
}
