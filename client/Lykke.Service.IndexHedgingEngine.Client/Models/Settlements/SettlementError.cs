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
        NotEnoughFunds,

        /// <summary>
        /// Indicates that there are no quote for this asset.
        /// </summary>
        NoQuote,

        /// <summary>
        /// Indicates that unknown error occurred while processing settlement.
        /// </summary>
        Unknown
    }
}
