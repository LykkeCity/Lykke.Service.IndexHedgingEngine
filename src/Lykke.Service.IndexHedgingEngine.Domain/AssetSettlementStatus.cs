namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Specifies a status of asset settlement.
    /// </summary>
    public enum AssetSettlementStatus
    {
        /// <summary>
        /// Unspecified status.
        /// </summary>
        None,
        
        /// <summary>
        /// The asset settlement created and waiting for process.
        /// </summary>
        New,
        
        /// <summary>
        /// Indicates that an error occurred during calculation or processing asset settlement.
        /// </summary>
        Error
    }
}
