using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Rabbit.Contracts.LykkeOrderBooks
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class LykkeLimitOrder
    {
        public string Id { get; set; }

        public string ClientId { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }
    }
}
