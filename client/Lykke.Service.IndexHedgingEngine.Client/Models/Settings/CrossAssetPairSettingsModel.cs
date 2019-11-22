using System;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settings
{
    /// <summary>
    /// Represents cross asset pairs setting.
    /// </summary>
    public class CrossAssetPairSettingsModel
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of base asset.
        /// </summary>
        public string BaseAsset { get; set; }

        /// <summary>
        /// Name of quote asset.
        /// </summary>
        public string QuoteAsset { get; set; }

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
