using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settlements
{
    /// <summary>
    /// Specifies a status of asset settlement.
    /// </summary>
    [PublicAPI]
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
        /// The asset amount transferred to the transit wallet.
        /// </summary>
        Reserved,
        
        /// <summary>
        /// The asset amount transferred to the client.
        /// </summary>
        Transferred,
        
        /// <summary>
        /// The reserved asset amount transferred back to the market maker wallet.
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// The asset settlement is completed.
        /// </summary>
        Completed
    }
}
