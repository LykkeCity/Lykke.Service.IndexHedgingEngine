namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Specifies a status of settlement.
    /// </summary>
    public enum SettlementStatus
    {
        /// <summary>
        /// Unspecified status.
        /// </summary>
        None,
        
        /// <summary>
        /// The settlement created and waiting for approve.
        /// </summary>
        New,
        
        /// <summary>
        /// The settlement is approved.
        /// </summary>
        Approved,
        
        /// <summary>
        /// The settlement is rejected.
        /// </summary>
        Rejected,
        
        /// <summary>
        /// The settlement is completed.
        /// </summary>
        Completed
    }
}
