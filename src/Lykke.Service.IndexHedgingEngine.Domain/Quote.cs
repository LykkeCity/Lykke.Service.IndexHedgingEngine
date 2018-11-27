using System;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents a quote of the asset pair.
    /// </summary>
    public class Quote
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Quote"/> with parameters.
        /// </summary>
        /// <param name="assetPairId"></param>
        /// <param name="time"></param>
        /// <param name="ask"></param>
        /// <param name="bid"></param>
        /// <param name="source"></param>
        public Quote(string assetPairId, DateTime time, decimal ask, decimal bid, string source)
        {
            AssetPairId = assetPairId;
            Time = time;
            Ask = ask;
            Bid = bid;
            Mid = (ask + bid) / 2m;
            Spread = Ask - Bid;
            Source = source;
        }

        /// <summary>
        /// The identifier of the asset pair.
        /// </summary>
        public string AssetPairId { get; }

        /// <summary>
        /// The time of price.
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        /// The best sell price in order book.
        /// </summary>
        public decimal Ask { get; }

        /// <summary>
        /// The best buy price in order book.
        /// </summary>
        public decimal Bid { get; }

        /// <summary>
        /// The mid price in order book.
        /// </summary>
        public decimal Mid { get; }

        /// <summary>
        /// The spread of order book.
        /// </summary>
        public decimal Spread { get; }

        /// <summary>
        /// The name of exchange.
        /// </summary>
        public string Source { get; }
    }
}
