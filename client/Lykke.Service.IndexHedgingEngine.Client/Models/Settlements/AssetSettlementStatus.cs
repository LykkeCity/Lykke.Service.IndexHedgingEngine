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
        /// Indicates that an error occurred during calculation or processing asset settlement.
        /// </summary>
        Error
    }
}
