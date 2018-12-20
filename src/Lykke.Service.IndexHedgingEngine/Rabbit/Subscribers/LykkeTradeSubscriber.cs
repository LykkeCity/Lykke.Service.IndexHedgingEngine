using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.MatchingEngine.Connector.Models.Events.Common;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Deduplication;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Handlers;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;
using Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.Rabbit.Subscribers;

namespace Lykke.Service.IndexHedgingEngine.Rabbit.Subscribers
{
    public class LykkeTradeSubscriber : IDisposable
    {
        private readonly SubscriberSettings _settings;
        private readonly ISettingsService _settingsService;
        private readonly IInternalTradeHandler[] _internalTradeHandlers;
        private readonly IDeduplicator _deduplicator;
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        private IStopable _subscriber;

        public LykkeTradeSubscriber(
            SubscriberSettings settings,
            ISettingsService settingsService,
            IInternalTradeHandler[] internalTradeHandlers,
            IDeduplicator deduplicator,
            ILogFactory logFactory)
        {
            _settings = settings;
            _settingsService = settingsService;
            _internalTradeHandlers = internalTradeHandlers;
            _deduplicator = deduplicator;
            _logFactory = logFactory;

            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForSubscriber(_settings.ConnectionString, _settings.Exchange, _settings.Queue)
                .UseRoutingKey(((int) MessageType.Order).ToString())
                .MakeDurable();

            _subscriber = new RabbitMqSubscriber<ExecutionEvent>(
                    _logFactory, settings, new ResilientErrorHandlingStrategy(_logFactory, settings,
                        TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new ProtobufMessageDeserializer<ExecutionEvent>())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .SetAlternativeExchange(_settings.AlternateConnectionString)
                .SetDeduplicator(_deduplicator)
                .Start();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        public void Dispose()
        {
            Stop();

            _subscriber?.Dispose();
        }

        private async Task ProcessMessageAsync(ExecutionEvent message)
        {
            if (message.Header.MessageType != MessageType.Order)
                return;

            string walletId = await _settingsService.GetWalletIdAsync();

            Order[] orders = message.Orders
                .Where(o => o.WalletId == walletId)
                .ToArray();

            if (orders.Any())
                _log.Info("Lykke trades received", message);

            orders = orders
                .Where(o => o.Side != OrderSide.UnknownOrderSide)
                .Where(o => o.Trades?.Count > 0)
                .ToArray();

            if (orders.Any())
            {
                try
                {
                    var internalTrades = new List<InternalTrade>();

                    foreach (Order order in orders)
                    {
                        // The limit order fully executed. The remaining volume is zero.
                        if (order.Status == OrderStatus.Matched)
                            internalTrades.AddRange(Map(order));

                        // The limit order partially executed.
                        if (order.Status == OrderStatus.PartiallyMatched)
                            internalTrades.AddRange(Map(order));

                        // The limit order was cancelled by matching engine after processing trades.
                        // In this case order partially executed and remaining volume is less than min volume allowed by asset pair.
                        if (order.Status == OrderStatus.Cancelled)
                            internalTrades.AddRange(Map(order));
                    }

                    await Task.WhenAll(_internalTradeHandlers.Select(o => o.HandleInternalTradesAsync(internalTrades)));

                    _log.InfoWithDetails("Lykke trades handled", internalTrades);
                }
                catch (Exception exception)
                {
                    _log.ErrorWithDetails(exception, "An error occurred while processing trades", orders);
                    throw;
                }
            }
        }

        private static IReadOnlyCollection<InternalTrade> Map(Order order)
        {
            var reports = new List<InternalTrade>();

            for (int i = 0; i < order.Trades.Count; i++)
            {
                Trade trade = order.Trades[i];

                TradeType tradeType = order.Side == OrderSide.Sell
                    ? TradeType.Sell
                    : TradeType.Buy;

                reports.Add(new InternalTrade
                {
                    Id = trade.TradeId,
                    AssetPairId = order.AssetPairId,
                    ExchangeOrderId = order.Id,
                    LimitOrderId = order.ExternalId,
                    Type = tradeType,
                    Timestamp = trade.Timestamp,
                    Price = decimal.Parse(trade.Price),
                    Volume = Math.Abs(decimal.Parse(trade.BaseVolume)),
                    OppositeWalletId = trade.OppositeWalletId,
                    OppositeLimitOrderId = trade.OppositeOrderId,
                    OppositeVolume = Math.Abs(decimal.Parse(trade.QuotingVolume))
                });
            }

            return reports;
        }
    }
}
