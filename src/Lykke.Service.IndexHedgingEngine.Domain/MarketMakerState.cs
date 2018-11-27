using System;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Market maker state.
    /// </summary>
    public class MarketMakerState
    {
        /// <summary>
        /// Market maker status.
        /// </summary>
        public MarketMakerStatus Status { get; set; }

        /// <summary>
        /// Time since the status change.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
