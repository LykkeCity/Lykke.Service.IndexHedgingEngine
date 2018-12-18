using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.CryptoIndex.Contract;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Handlers;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;
using Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.Rabbit.Subscribers;

namespace Lykke.Service.IndexHedgingEngine.Rabbit.Subscribers
{
    [UsedImplicitly]
    public class IndexTickPriceSubscriber : IDisposable
    {
        private readonly SubscriberSettings _settings;
        private RabbitMqSubscriber<IndexTickPrice> _subscriber;
        private readonly IIndexHandler _indexHandler;
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        public IndexTickPriceSubscriber(
            SubscriberSettings settings,
            IIndexHandler indexHandler,
            ILogFactory logFactory)
        {
            _settings = settings;

            _indexHandler = indexHandler;
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForSubscriber(_settings.ConnectionString, _settings.Exchange, _settings.Queue);

            _subscriber = new RabbitMqSubscriber<IndexTickPrice>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings, TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<IndexTickPrice>())
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

        private Task ProcessMessageAsync(IndexTickPrice message)
        {
            try
            {
                var index = new Index(message.AssetPair, message.Ask, message.Source, message.Timestamp,
                    message.AssetsInfo
                        .Select(o => new AssetWeight(o.AssetId, o.Weight, o.Price, o.IsDisabled))
                        .ToArray());

                _indexHandler.HandleIndexAsync(index);

                _log.InfoWithDetails("Index price handled", message);
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, "An error occurred while processing index tick price", message);
                throw;
            }

            return Task.CompletedTask;
        }
    }
}
