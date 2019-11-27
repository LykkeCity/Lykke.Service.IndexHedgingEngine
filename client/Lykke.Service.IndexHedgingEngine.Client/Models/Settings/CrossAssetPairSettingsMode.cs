namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settings
{
    /// <summary>
    /// Represents cross asset pairs setting mode.
    /// </summary>
    public enum CrossAssetPairSettingsMode
    {
        /// <summary>
        /// Unspecified mode.
        /// </summary>
        None,

        /// <summary>
        /// Cross asset pairs is used to calculate its orders.
        /// </summary>
        Enabled,

        /// <summary>
        /// Cross asset pairs is not used to calculate its orders.
        /// </summary>
        Disabled
    }
}
