using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.PrimaryMarket
{
    /// <summary>
    /// Represents the balance item of Primary Market wallet.
    /// </summary>
    [PublicAPI]
    public class PrimaryMarketBalanceModel
    {
        /// <summary>
        /// Id of the asset.
        /// </summary>
        public string AssetId { set; get; }
        
        /// <summary>
        /// Available balance of the asset.
        /// </summary>
        public decimal Balance { set; get; }
        
        /// <summary>
        /// The reserved amount.
        /// </summary>
        public decimal Reserved { get; set; }
    }
}
