using System;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Trades
{
    /// <summary>
    /// Represents a trade that was executed on virtual exchange.
    /// </summary>
    [PublicAPI]
    public class VirtualTradeModel
    {
        /// <summary>
        /// The identifier of the trade.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The internal identifier of the limit order which executed while trade.
        /// </summary>
        public string LimitOrderId { get; set; }
        
        /// <summary>
        /// The identifier of asset.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The identifier of asset pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The type of the trade.
        /// </summary>
        public TradeType Type { get; set; }

        /// <summary>
        /// The time of the trade.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The price of the trade.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The volume of the trade.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// The opposite volume of the trade.
        /// </summary>
        public decimal OppositeVolume { get; set; }
    }
}
