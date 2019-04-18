namespace Lykke.Service.IndexHedgingEngine.Domain.Settings
{
    /// <summary>
    /// Represents asset setting for exchange.
    /// </summary>
    public class AssetSettings
    {
        /// <summary>
        /// The name of asset.
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// The name of exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The identifier of the asset used for exchange.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The accuracy of the asset (mainly used to display balance on UI).
        /// </summary>
        public int Accuracy { get; set; }
    }
}
