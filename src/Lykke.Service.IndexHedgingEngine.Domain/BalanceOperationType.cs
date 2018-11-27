namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Specifies a type of a balance operation.
    /// </summary>
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
