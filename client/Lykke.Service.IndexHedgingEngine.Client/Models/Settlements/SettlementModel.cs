using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settlements
{
    /// <summary>
    /// Represents a token settlement for client.
    /// </summary>
    [PublicAPI]
    public class SettlementModel
    {
        /// <summary>
        /// The identifier of settlement.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// The amount of token.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The price in USD of the token that was while requesting the settlement.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The identifier of target Lykke wallet.
        /// </summary>
        public string WalletId { get; set; }

        /// <summary>
        /// The identifier of the client.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The additional information about settlement.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// If <c>true</c> the assets should be settle direct, otherwise in USD. 
        /// </summary>
        public bool IsDirect { get; set; }
        
        /// <summary>
        /// The status of the settlement.
        /// </summary>
        public SettlementStatus Status { get; set; }

        /// <summary>
        /// The error that occurred while processing the settlement.  
        /// </summary>
        public SettlementError Error { get; set; }

        /// <summary>
        /// The identifier of the user who requested settlement.
        /// </summary>
        public string CreatedBy { get; set; }
        
        /// <summary>
        /// The creation date of the settlement.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The collection of asset to settle.
        /// </summary>
        public IReadOnlyCollection<AssetSettlementModel> Assets { get; set; }
    }
}
