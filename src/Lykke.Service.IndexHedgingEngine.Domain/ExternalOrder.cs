namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents a mapping between hedge a limit order and an order from an external exchange.
    /// </summary>
    public class ExternalOrder
    {
        public ExternalOrder()
        {
        }

        public ExternalOrder(string id, string exchange, string asset, string hedgeLimitOrderId)
        {
            Id = id;
            Exchange = exchange;
            Asset = asset;
            HedgeLimitOrderId = hedgeLimitOrderId;
        }
        
        /// <summary>
        /// The unique identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of exchange where order created.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The asset that associated with order.
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// The identifier of hedge limit order that used to created external order.
        /// </summary>
        public string HedgeLimitOrderId { get; set; }
    }
}
