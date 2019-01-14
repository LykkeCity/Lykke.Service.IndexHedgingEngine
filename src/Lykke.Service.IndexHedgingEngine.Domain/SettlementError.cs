namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Specifies settlement error.
    /// </summary>
    public enum SettlementError
    {
        /// <summary>
        /// Unspecified error.
        /// </summary>
        None,
        
        /// <summary>
        /// Indicates that there are no funds to reserve.
        /// </summary>
        NotEnoughFunds,
        
        /// <summary>
        /// Indicates that unknown error occurred while processing settlement.
        /// </summary>
        Unknown
    }
}
