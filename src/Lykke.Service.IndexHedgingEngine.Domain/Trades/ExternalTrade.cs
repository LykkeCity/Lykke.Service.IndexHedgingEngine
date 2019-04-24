using System;

namespace Lykke.Service.IndexHedgingEngine.Domain.Trades
{
    /// <summary>
    /// Represents an external trade.
    /// </summary>
    public class ExternalTrade
    {
        /// <summary>
        /// The identifier of the trade.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The internal identifier of the limit order which executed while trade.
        /// </summary>
        public string LimitOrderId { get; set; }

        /// <summary>
        /// The exchange identifier of the limit order which executed while trade.
        /// </summary>
        public string ExchangeOrderId { get; set; }

        /// <summary>
        /// The asset pair.
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
        /// The status of the trade.
        /// </summary>
        public TradeStatus Status { get; set; }

        /// <summary>
        /// The original volume of the limit order.
        /// </summary>
        public decimal OriginalVolume { get; set; }
        
        /// <summary>
        /// The remaining volume of the limit order.
        /// </summary>
        public decimal RemainingVolume { get; set; }
    }
}
