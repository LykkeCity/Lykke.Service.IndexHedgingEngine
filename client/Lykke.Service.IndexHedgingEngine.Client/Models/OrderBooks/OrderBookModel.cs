using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.OrderBooks
{
    /// <summary>
    /// Represents an order book.
    /// </summary>
    [PublicAPI]
    public class OrderBookModel
    {
        /// <summary>
        /// The asset pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The date and time of creation.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The collection of limit orders.
        /// </summary>
        public IReadOnlyCollection<LimitOrderModel> LimitOrders { get; set; }
    }
}
