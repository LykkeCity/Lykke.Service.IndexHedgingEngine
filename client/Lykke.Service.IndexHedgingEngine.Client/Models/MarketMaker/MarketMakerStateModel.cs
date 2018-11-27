using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.MarketMaker
{
    /// <summary>
    /// Market maker state.
    /// </summary>
    [PublicAPI]
    public class MarketMakerStateModel
    {
        /// <summary>
        /// Market maker status.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MarketMakerStatus Status { get; set; }

        /// <summary>
        /// Time since the status change.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
