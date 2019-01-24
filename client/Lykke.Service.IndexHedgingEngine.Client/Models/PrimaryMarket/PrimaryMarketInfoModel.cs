using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.PrimaryMarket
{
    /// <summary>
    /// Represents general information about the Primary Market.
    /// </summary>
    [PublicAPI]
    public class PrimaryMarketInfoModel
    {
        /// <summary>
        /// The Id of the wallet that is used as Primary Market wallet.
        /// </summary>
        public string WalletId { set; get; }
    }
}
