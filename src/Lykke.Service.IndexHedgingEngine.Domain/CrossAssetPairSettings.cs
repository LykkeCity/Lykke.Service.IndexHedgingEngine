using System;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents cross asset pairs setting.
    /// </summary>
    public class CrossAssetPairSettings
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Base asset identifier.
        /// </summary>
        public string BaseAssetId { get; set; }

        /// <summary>
        /// Quote asset identifier.
        /// </summary>
        public string QuoteAssetId { get; set; }

        /// <summary>
        /// Identifier of the asset pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// Spread on 'buy' side.
        /// </summary>
        public decimal BuySpread { get; set; }

        /// <summary>
        /// Volume on 'buy' side.
        /// </summary>
        public decimal BuyVolume { get; set; }

        /// <summary>
        /// Spread on 'sell' side.
        /// </summary>
        public decimal SellSpread { get; set; }

        /// <summary>
        /// Volume on 'sell' side.
        /// </summary>
        public decimal SellVolume { get; set; }

        /// <summary>
        /// Is cross pair enabled or disabled.
        /// </summary>
        public CrossAssetPairSettingsMode Mode { get; set; }
    }
}
