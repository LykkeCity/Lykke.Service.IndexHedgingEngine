using System;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.PrimaryMarket
{
    /// <summary>
    /// Represents balance update entity.
    /// </summary>
    [PublicAPI]
    public class PrimaryMarketBalanceChangeModel
    {
        /// <summary>
        /// Date and time of when balance update was performed.
        /// </summary>
        public DateTime DateTime { set; get; }
        
        /// <summary>
        /// Id of the asset who's balance is being updated.
        /// </summary>
        public string AssetId { set; get; }
        
        /// <summary>
        /// Amount by which the balance should be changed.
        /// </summary>
        public decimal Amount { set; get; }
        
        /// <summary>
        /// User that performs the update.
        /// </summary>
        public string UserId { set; get; }
        
        /// <summary>
        /// Comment on why the change is performed.
        /// </summary>
        public string Comment { set; get; }
    }
}
