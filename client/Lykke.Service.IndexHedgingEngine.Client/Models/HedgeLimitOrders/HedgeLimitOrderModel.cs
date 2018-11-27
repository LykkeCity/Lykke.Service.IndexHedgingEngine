using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.HedgeLimitOrders
{
    /// <summary>
    /// Represents a hedge limit order details.
    /// </summary>
    [PublicAPI]
    public class HedgeLimitOrderModel
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
        [JsonConverter(typeof(StringEnumConverter))]
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
        /// The error details.
        /// </summary>
        public string Error { get; set; }
    }
}
