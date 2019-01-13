using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.IndexHedgingEngine.DomainServices.Timers;
using Lykke.Service.IndexHedgingEngine.Rabbit.Subscribers;

namespace Lykke.Service.IndexHedgingEngine.Managers
{
    [UsedImplicitly]
    public class ShutdownManager : IShutdownManager
    {
        private readonly LykkeTradeSubscriber _lykkeTradeSubscriber;
        private readonly LykkeOrderBookSubscriber _lykkeOrderBookSubscriber;
        private readonly IndexTickPriceSubscriber _indexTickPriceSubscriber;
        private readonly LykkeBalancesTimer _lykkeBalancesTimer;
        private readonly SettlementsTimer _settlementsTimer;
        private readonly QuoteSubscriber[] _quoteSubscribers;

        public ShutdownManager(
            LykkeTradeSubscriber lykkeTradeSubscriber,
            LykkeOrderBookSubscriber lykkeOrderBookSubscriber,
            IndexTickPriceSubscriber indexTickPriceSubscriber,
            LykkeBalancesTimer lykkeBalancesTimer,
            SettlementsTimer settlementsTimer,
            QuoteSubscriber[] quoteSubscribers)
        {
            _lykkeTradeSubscriber = lykkeTradeSubscriber;
            _lykkeOrderBookSubscriber = lykkeOrderBookSubscriber;
            _indexTickPriceSubscriber = indexTickPriceSubscriber;
            _lykkeBalancesTimer = lykkeBalancesTimer;
            _settlementsTimer = settlementsTimer;
            _quoteSubscribers = quoteSubscribers;
        }

        public Task StopAsync()
        {
            _settlementsTimer.Stop();
            
            _indexTickPriceSubscriber.Stop();

            _lykkeOrderBookSubscriber.Stop();

            _lykkeTradeSubscriber.Stop();

            _lykkeBalancesTimer.Stop();

            foreach (QuoteSubscriber quoteSubscriber in _quoteSubscribers)
                quoteSubscriber.Stop();

            return Task.CompletedTask;
        }
    }
}
