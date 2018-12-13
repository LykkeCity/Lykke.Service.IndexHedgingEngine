using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.Rabbit.Subscribers.Quotes;

namespace Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.Rabbit.Subscribers
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class RabbitSubscribers
    {
        public SubscriberSettings LykkeTrades { get; set; }

        public SubscriberSettings LykkeOrderBooks { get; set; }
        
        public SubscriberSettings IndexTickPrices { get; set; }
        
        public QuotesSettings Quotes { get; set; }
    }
}
