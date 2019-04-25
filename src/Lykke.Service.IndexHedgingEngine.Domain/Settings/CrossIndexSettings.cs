using System;

namespace Lykke.Service.IndexHedgingEngine.Domain.Settings
{
    /// <summary>
    /// Represents cross asset pair setting.
    /// </summary>
    public class CrossIndexSettings
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// The identifier of the original index asset pair (LyCI/USD).
        /// </summary>
        public string OriginalAssetPairId { get; set; }

        /// <summary>
        /// The name of the cross asset pair exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The identifier of the cross asset pair (EUR/USD).
        /// </summary>
        public string CrossAssetPairId { get; set; }

        /// <summary>
        /// If cross asset pair must be inverted.
        /// </summary>
        public bool IsInverted { get; set; }

        /// <summary>
        /// The identifier of the resulting index asset pair (LyCI/EUR).
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The identifier of the resulting index base asset (LyCI).
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The identifier of the resulting index quote asset (EUR).
        /// </summary>
        public string QuoteAssetId { get; set; }
    }
}
