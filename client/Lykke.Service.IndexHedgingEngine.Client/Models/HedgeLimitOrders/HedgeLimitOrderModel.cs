using System;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.OrderBooks;
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
        /// The type of the limit order price.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PriceType PriceType { get; set; }
        
        /// <summary>
        /// The limit order price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The limit order volume.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// The code of the error which occurred while processing on ME.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LimitOrderError Error { get; set; }

        /// <summary>
        /// The error details.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
