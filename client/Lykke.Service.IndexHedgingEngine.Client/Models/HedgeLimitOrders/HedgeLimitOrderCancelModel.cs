using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.HedgeLimitOrders
{
    /// <summary>
    /// Represents a hedge limit order cancellation details.
    /// </summary>
    [PublicAPI]
    public class HedgeLimitOrderCancelModel
    {
        /// <summary>
        /// The name of the exchange.
        /// </summary>
        public string Exchange { get; set; }
        
        /// <summary>
        /// The identifier of asset.
        /// </summary>
        public string AssetId { get; set; }
    }
}
