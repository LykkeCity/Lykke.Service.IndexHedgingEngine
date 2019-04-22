using System;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settings
{
    /// <summary>
    /// Represents cross asset pair setting.
    /// </summary>
    [PublicAPI]
    public class CrossAssetPairSettingsModel
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// The identifier of the original asset pair.
        /// </summary>
        public string IndexAssetPairId { get; set; }

        /// <summary>
        /// The name of the cross asset pair exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The identifier of the cross asset pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// If cross asset pair must be inverted
        /// </summary>
        public bool IsInverted { get; set; }
    }
}
