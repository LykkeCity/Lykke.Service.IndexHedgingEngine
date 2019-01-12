using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settlements
{
    /// <summary>
    /// Specifies settlement error.
    /// </summary>
    [PublicAPI]
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
