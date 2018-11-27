using System;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents a hedge limit order details.
    /// </summary>
    public class HedgeLimitOrder
    {
        /// <summary>
        /// The identifier of the limit order.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the exchange.
        /// </summary>
        public string Exchange { get; set; }
        
        /// <summary>
        /// The identifier of asset.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The identifier of asset pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The limit order type.
        /// </summary>
        public LimitOrderType Type { get; set; }

        /// <summary>
        /// The creation time of limit order.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The limit order price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The limit order volume.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Contains information of hedge limit order creation.
        /// </summary>
        public string Context { get; set; }

        public static HedgeLimitOrder Create(string exchange, string assetId, string assetPairId,
            LimitOrderType limitOrderType, decimal price, decimal volume)
        {
            return new HedgeLimitOrder
            {
                Id = Guid.NewGuid().ToString("D"),
                Exchange = exchange,
                AssetId = assetId,
                AssetPairId = assetPairId,
                Type = limitOrderType,
                Timestamp = DateTime.UtcNow,
                Price = price,
                Volume = volume
            };
        }
    }
}
