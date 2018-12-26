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
        /// The type of the limit order price.
        /// </summary>
        public PriceType PriceType { get; set; }

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
        
        /// <summary>
        /// The code of the error which occurred while processing on ME.
        /// </summary>
        public LimitOrderError Error { get; set; }

        /// <summary>
        /// The error details.
        /// </summary>
        public string ErrorMessage { get; set; }

        public void ExecuteVolume(decimal volume)
        {
            Volume -= volume;
        }

        public static HedgeLimitOrder Create(string exchange, string assetId, string assetPairId,
            LimitOrderType limitOrderType, PriceType priceType, decimal price, decimal volume)
        {
            return new HedgeLimitOrder
            {
                Id = Guid.NewGuid().ToString("D"),
                Exchange = exchange,
                AssetId = assetId,
                AssetPairId = assetPairId,
                Type = limitOrderType,
                PriceType = priceType,
                Timestamp = DateTime.UtcNow,
                Price = price,
                Volume = volume
            };
        }
    }
}
