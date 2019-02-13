using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models
{
    /// <summary>
    /// Represents a quote.
    /// </summary>
    [PublicAPI]
    public class QuoteModel
    {
        /// <summary>
        /// The identifier of the asset pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The best sell price in order book.
        /// </summary>
        public decimal Ask { get; set; }

        /// <summary>
        /// The best buy price in order book.
        /// </summary>
        public decimal Bid { get; set; }

        /// <summary>
        /// The mid price in order book.
        /// </summary>
        public decimal Mid { get; set; }

        /// <summary>
        /// The name of exchange.
        /// </summary>
        public string Source { get; set; }
    }
}
