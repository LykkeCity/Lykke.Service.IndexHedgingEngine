using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Client;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Sdk;
using Lykke.Service.Balances.Client;
using Lykke.Service.ExchangeOperations.Client;
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
            var assetPairMapping = new Dictionary<string, string>
            {
                {"EOScoinUSD", "EOSUSD"}
            };

            builder.RegisterModule(new DomainServices.AutofacModule(
                _settings.CurrentValue.IndexHedgingEngineService.Name,
                _settings.CurrentValue.IndexHedgingEngineService.WalletId,
                _settings.CurrentValue.IndexHedgingEngineService.TransitWalletId,
                _settings.CurrentValue.IndexHedgingEngineService.PrimaryMarketWalletId,
                assetPairMapping,
                _settings.CurrentValue.IndexHedgingEngineService.ExchangeAdapters.Select(o => o.Name).ToList()));
            builder.RegisterModule(new AzureRepositories.AutofacModule(
                _settings.Nested(o => o.IndexHedgingEngineService.Db
                    .DataConnectionString),
                _settings.Nested(o => o.IndexHedgingEngineService.Db
                    .LykkeTradesMeQueuesDeduplicatorConnectionString)));

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            RegisterRabbit(builder, assetPairMapping);
            RegisterClients(builder);
        }

        private void RegisterRabbit(ContainerBuilder builder, IReadOnlyDictionary<string, string> assetPairMapping)
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
                    .WithParameter(TypedParameter.From(assetPairMapping))
                    .SingleInstance();
            }
        }

        private void RegisterClients(ContainerBuilder builder)
        {
            IReadOnlyDictionary<string, AdapterEndpoint> endpoints =
                _settings.CurrentValue.IndexHedgingEngineService.ExchangeAdapters.ToDictionary(o => o.Name,
                    v => new AdapterEndpoint(v.ApiKey, new Uri(v.Url)));

            builder.RegisterInstance(new ExchangeAdapterClientFactory(endpoints));

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

            builder.RegisterMeClient(new MeClientSettings{Endpoint = endPoint, EnableRetries = true});
        }
    }
}
