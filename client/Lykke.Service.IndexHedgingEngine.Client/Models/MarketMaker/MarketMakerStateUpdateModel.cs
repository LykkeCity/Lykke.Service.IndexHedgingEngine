using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.MarketMaker
{
    /// <summary>
    /// Describes a new state of market maker.
    /// </summary>
    [PublicAPI]
    public class MarketMakerStateUpdateModel
    {
        /// <summary>
        /// The market maker status that should be applied.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MarketMakerStatus Status { get; set; }

        /// <summary>
        /// The comment that describes a reason of changes.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The identifier of the user who made changes.
        /// </summary>
        public string UserId { get; set; }
    }
}
