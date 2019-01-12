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
        NotEnoughFunds
    }
}
