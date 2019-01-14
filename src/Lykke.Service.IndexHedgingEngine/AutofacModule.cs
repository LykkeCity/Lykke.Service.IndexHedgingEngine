using System.Linq;
using System.Net;
using Autofac;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.Balances.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Managers;
using Lykke.Service.IndexHedgingEngine.Rabbit.Subscribers;
using Lykke.Service.IndexHedgingEngine.Settings;
using Lykke.Service.IndexHedgingEngine.Settings.Clients.MatchingEngine;
using Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.Rabbit.Subscribers;
using Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.Rabbit.Subscribers.Quotes;
using Lykke.SettingsReader;

namespace Lykke.Service.IndexHedgingEngine
{
    [UsedImplicitly]
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public AutofacModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new DomainServices.AutofacModule(
                _settings.CurrentValue.IndexHedgingEngineService.Name,
                _settings.CurrentValue.IndexHedgingEngineService.WalletId,
                _settings.CurrentValue.IndexHedgingEngineService.TransitWalletId,
                _settings.CurrentValue.IndexHedgingEngineService.Rabbit.Subscribers.Quotes.Exchanges
                    .Where(o => o.Name != ExchangeNames.Lykke)
                    .Select(o => new ExchangeSettings
                    {
                        Name = o.Name,
                        Fee = decimal.Zero,
                        HasApi = true
                    })
                    .ToArray()));
            builder.RegisterModule(new AzureRepositories.AutofacModule(
                _settings.Nested(o => o.IndexHedgingEngineService.Db
                    .DataConnectionString),
                _settings.Nested(o => o.IndexHedgingEngineService.Db
                    .LykkeTradesMeQueuesDeduplicatorConnectionString)));

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            RegisterRabbit(builder);
            RegisterClients(builder);
        }

        private void RegisterRabbit(ContainerBuilder builder)
        {
            builder.RegisterType<LykkeTradeSubscriber>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.IndexHedgingEngineService.Rabbit.Subscribers
                    .LykkeTrades))
                .AsSelf()
                .SingleInstance();
            
            builder.RegisterType<LykkeOrderBookSubscriber>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.IndexHedgingEngineService.Rabbit.Subscribers
                    .LykkeOrderBooks))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<IndexTickPriceSubscriber>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.IndexHedgingEngineService.Rabbit.Subscribers
                    .IndexTickPrices))
                .AsSelf()
                .SingleInstance();

            QuotesSettings quotesSettings = _settings.CurrentValue.IndexHedgingEngineService.Rabbit.Subscribers.Quotes;

            foreach (Exchange exchange in quotesSettings.Exchanges)
            {
                builder.RegisterType<QuoteSubscriber>()
                    .AsSelf()
                    .WithParameter(TypedParameter.From(new SubscriberSettings
                    {
                        Exchange = exchange.Endpoint,
                        Queue = quotesSettings.Queue,
                        ConnectionString = quotesSettings.ConnectionString
                    }))
                    .SingleInstance();
            }
        }

        private void RegisterClients(ContainerBuilder builder)
        {
            builder.RegisterBalancesClient(_settings.CurrentValue.BalancesServiceClient);

            builder.Register(container =>
                    new ExchangeOperationsServiceClient(_settings.CurrentValue.ExchangeOperationsServiceClient
                        .ServiceUrl))
                .As<IExchangeOperationsServiceClient>()
                .SingleInstance();

            MatchingEngineClientSettings matchingEngineClientSettings = _settings.CurrentValue.MatchingEngineClient;

            if (!IPAddress.TryParse(matchingEngineClientSettings.IpEndpoint.Host, out var address))
                address = Dns.GetHostAddressesAsync(matchingEngineClientSettings.IpEndpoint.Host).Result[0];

            var endPoint = new IPEndPoint(address, matchingEngineClientSettings.IpEndpoint.Port);

            builder.RegisgterMeClient(endPoint);
        }
    }
}
