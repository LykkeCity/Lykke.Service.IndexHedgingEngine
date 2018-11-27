namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Market maker status.
    /// </summary>
    public enum MarketMakerStatus
    {
        /// <summary>
        /// Unspecified type.
        /// </summary>
        None,

        /// <summary>
        /// Market maker is stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// Market maker is active.
        /// </summary>
        Active,

        /// <summary>
        /// Market maker is paused.
        /// </summary>
        Paused
    }
}
