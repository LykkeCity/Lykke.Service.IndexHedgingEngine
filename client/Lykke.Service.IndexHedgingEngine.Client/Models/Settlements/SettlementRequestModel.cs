using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settlements
{
    /// <summary>
    /// Represents the information about new settlement.
    /// </summary>
    [PublicAPI]
    public class SettlementRequestModel
    {
        /// <summary>
        /// The name of the token to settlement.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The amount of tokens requested to settle.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The additional information about settlement.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The identifier of the client wallet.
        /// </summary>
        public string WalletId { get; set; }

        /// <summary>
        /// The identifier of the client.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// If <c>true</c> the assets should be settle direct, otherwise in USD.
        /// </summary>
        public bool IsDirect { get; set; }
    }
}
