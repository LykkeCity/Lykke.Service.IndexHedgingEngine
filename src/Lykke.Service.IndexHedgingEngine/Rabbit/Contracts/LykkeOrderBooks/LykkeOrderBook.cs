using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.IndexHedgingEngine.Rabbit.Contracts.LykkeOrderBooks
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class LykkeOrderBook
    {
        [JsonProperty("assetPair")]
        public string AssetPairId { get; set; }

        public bool IsBuy { get; set; }

        public DateTime Timestamp { get; set; }

        [JsonProperty("prices")]
        public IReadOnlyList<LykkeLimitOrder> LimitOrders { get; set; }
    }
}
