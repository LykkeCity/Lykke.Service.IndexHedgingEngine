using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models
{
    /// <summary>
    /// Specifies a type of a balance operation.
    /// </summary>
    [PublicAPI]
    public enum BalanceOperationType
    {
        /// <summary>
        /// Unspecified type.
        /// </summary>
        None,

        /// <summary>
        /// An amount transferred to wallet.
        /// </summary>
        CashIn,

        /// <summary>
        /// An amount transferred from wallet.
        /// </summary>
        CashOut
    }
}
