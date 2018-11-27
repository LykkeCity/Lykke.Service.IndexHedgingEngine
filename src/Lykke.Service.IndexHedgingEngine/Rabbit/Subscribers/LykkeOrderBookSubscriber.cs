using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Rabbit.Contracts.LykkeOrderBooks;
using Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.Rabbit.Subscribers;

namespace Lykke.Service.IndexHedgingEngine.Rabbit.Subscribers
{
    [UsedImplicitly]
    public class LykkeOrderBookSubscriber : IDisposable
    {
        private readonly SubscriberSettings _settings;
        private readonly IOrderBookService _orderBookService;
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        private RabbitMqSubscriber<LykkeOrderBook> _subscriber;

        public LykkeOrderBookSubscriber(
            SubscriberSettings settings,
            IOrderBookService orderBookService,
            ILogFactory logFactory)
        {
            _settings = settings;
            _orderBookService = orderBookService;
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForSubscriber(_settings.ConnectionString, _settings.Exchange, _settings.Queue);

            settings.DeadLetterExchangeName = null;
            settings.IsDurable = false;

            _subscriber = new RabbitMqSubscriber<LykkeOrderBook>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings, TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<LykkeOrderBook>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        private async Task ProcessMessageAsync(LykkeOrderBook message)
        {
            try
            {
                var limitOrders = message.LimitOrders
                    .Select(o => new LimitOrder
                    {
                        Id = o.Id,
                        WalletId = o.ClientId,
                        Volume = Math.Abs(o.Volume),
                        Price = o.Price,
                        Type = message.IsBuy ? LimitOrderType.Buy : LimitOrderType.Sell
                    })
                    .ToArray();

                if (message.IsBuy)
                {
                    await _orderBookService.UpdateBuyLimitOrdersAsync(message.AssetPairId, message.Timestamp,
                        limitOrders);
                }
                else
                {
                    await _orderBookService.UpdateSellLimitOrdersAsync(message.AssetPairId, message.Timestamp,
                        limitOrders);
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception, "An error occurred during processing lykke order book", message);
            }
        }
    }
}
