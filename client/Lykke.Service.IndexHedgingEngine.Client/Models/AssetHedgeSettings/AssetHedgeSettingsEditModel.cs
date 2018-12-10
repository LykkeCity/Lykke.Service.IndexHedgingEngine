using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.AssetHedgeSettings
{
    /// <summary>
    /// Represents an asset hedge settings.
    /// </summary>
    [PublicAPI]
    public class AssetHedgeSettingsEditModel
    {
        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The name of hedge exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The identifier of the external asset pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The min volume that can be used to create external limit order.
        /// </summary>
        public decimal MinVolume { get; set; }

        /// <summary>
        /// The accuracy of the hedge limit order volume.
        /// </summary>
        public int VolumeAccuracy { get; set; }

        /// <summary>
        /// The accuracy of the hedge limit order price.
        /// </summary>
        public int PriceAccuracy { get; set; }

        /// <summary>
        /// The asset hedging mode.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AssetHedgeMode Mode { get; set; }
    }
}
