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
        /// The asset hedging mode.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AssetHedgeMode Mode { get; set; }
        
        /// <summary>
        /// The name of the exchange that used to control deviation of hedge limit order price.
        /// </summary>
        public string ReferenceExchange { get; set; }
        
        /// <summary>
        /// The allowed deviation of hedge limit order price in comparison with <see cref="ReferenceExchange"/>.
        /// </summary>
        public decimal? ReferenceDelta { get; set; }
    }
}
