namespace Lykke.Service.IndexHedgingEngine.Client.Models.Reports
{
    /// <summary>
    /// Represents an asset investment details.
    /// </summary>
    public class AssetInvestmentModel
    {
        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string AssetId { get; set; }
        
        /// <summary>
        /// The distributed amount of the asset.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// The remaining volume of current position.
        /// </summary>
        public decimal RemainingAmount { get; set; }
    }
}
