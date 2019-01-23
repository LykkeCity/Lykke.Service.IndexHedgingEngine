using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.MatchingEngine.Connector.Models.Common;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;
using Lykke.Service.IndexHedgingEngine.DomainServices.Utils;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Exchanges
{
    [UsedImplicitly]
    public class LykkeExchangeService : ILykkeExchangeService
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IExchangeOperationsServiceClient _exchangeOperationsServiceClient;
        private readonly ISettingsService _settingsService;
        private readonly ILog _log;

        public LykkeExchangeService(
            IMatchingEngineClient matchingEngineClient,
            IExchangeOperationsServiceClient exchangeOperationsServiceClient,
            ISettingsService settingsService,
            ILogFactory logFactory)
        {
            _matchingEngineClient = matchingEngineClient;
            _exchangeOperationsServiceClient = exchangeOperationsServiceClient;
            _settingsService = settingsService;
            _log = logFactory.CreateLog(this);
        }

        public Task ApplyAsync(string assetPairId, LimitOrder limitOrder)
        {
            return ApplyAsync(assetPairId, new[] {limitOrder});
        }

        public async Task ApplyAsync(string assetPairId, IReadOnlyCollection<LimitOrder> limitOrders)
        {
            string walletId = _settingsService.GetWalletId();

            if (string.IsNullOrEmpty(walletId))
                throw new ExchangeException("The wallet not set");

            var multiOrderItems = new List<MultiOrderItemModel>();

            foreach (LimitOrder limitOrder in limitOrders)
            {
                var multiOrderItem = new MultiOrderItemModel
                {
                    Id = limitOrder.Id,
                    OrderAction = ToOrderAction(limitOrder.Type),
                    Price = (double) limitOrder.Price,
                    Volume = (double) Math.Abs(limitOrder.Volume)
                };

                multiOrderItems.Add(multiOrderItem);
            }

            var multiLimitOrder = new MultiLimitOrderModel
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = walletId,
                AssetPairId = assetPairId,
                CancelPreviousOrders = true,
                Orders = multiOrderItems,
                CancelMode = CancelMode.BothSides
            };

            _log.InfoWithDetails("Matching engine place multi limit order request", multiLimitOrder);

            MultiLimitOrderResponse response;

            try
            {
                response = await _matchingEngineClient.PlaceMultiLimitOrderAsync(multiLimitOrder);
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, "An error occurred during creating limit orders", multiLimitOrder);
                
                throw new ExchangeException("Cannot create limit orders an unexpected error occurred", exception);
            }

            if (response == null)
                throw new ExchangeException("Matching engine returned an empty response");

            foreach (LimitOrderStatusModel orderStatus in response.Statuses)
            {
                LimitOrder limitOrder = limitOrders.SingleOrDefault(e => e.Id == orderStatus.Id);

                if (limitOrder != null)
                {
                    limitOrder.Error = ToLimitOrderError(orderStatus.Status);
                    limitOrder.ErrorMessage = limitOrder.Error != LimitOrderError.Unknown
                        ? orderStatus.StatusReason
                        : !string.IsNullOrEmpty(orderStatus.StatusReason)
                            ? orderStatus.StatusReason
                            : "Unknown error";
                }
                else
                {
                    _log.WarningWithDetails("Matching engine returned status for unknown limit order",
                        new {LimitOrderId = orderStatus.Id});
                }
            }

            string[] ignoredLimitOrders = response.Statuses
                .Select(x => x.Id)
                .Except(multiLimitOrder.Orders.Select(x => x.Id))
                .ToArray();

            if (ignoredLimitOrders.Any())
            {
                _log.WarningWithDetails("Matching engine response not contains status of limit order",
                    new {AssetPairId = assetPairId, LimitOrders = ignoredLimitOrders});
            }

            _log.InfoWithDetails("Matching engine place multi limit order response", response);
        }

        public async Task CancelAsync(string assetPairId)
        {
            string walletId = _settingsService.GetWalletId();

            if (string.IsNullOrEmpty(walletId))
                throw new ExchangeException("The wallet not set");
            
            var multiLimitOrder = new MultiLimitOrderModel
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = walletId,
                AssetPairId = assetPairId,
                CancelPreviousOrders = true,
                Orders = new List<MultiOrderItemModel>(),
                CancelMode = CancelMode.BothSides
            };

            _log.InfoWithDetails("Matching engine cancel multi limit order request", multiLimitOrder);

            MultiLimitOrderResponse response;

            try
            {
                response = await _matchingEngineClient.PlaceMultiLimitOrderAsync(multiLimitOrder);
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, "An error occurred during cancelling limit orders",
                    multiLimitOrder);
                
                throw new ExchangeException("Cannot cancel limit orders an unexpected error occurred", exception);
            }

            if (response == null)
                throw new ExchangeException("Matching engine returned an empty response");

            _log.InfoWithDetails("Matching engine cancel multi limit order response", response);
        }
        
        public async Task<string> CashInAsync(string walletId, string assetId, decimal amount, string userId,
            string comment)
        {
            _log.InfoWithDetails("Cash in request", new {walletId, assetId, amount, userId, comment});

            ExchangeOperationResult result;

            try
            {
                result = await RetriesWrapper.RunWithRetriesAsync(() => _exchangeOperationsServiceClient
                    .ManualCashInAsync(walletId, assetId, (double) amount, userId, comment));
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, new {walletId, assetId, amount, userId, comment});

                throw new ExchangeException("An error occurred while processing cash in request", exception);
            }

            _log.InfoWithDetails("Cash in response", result);

            if (result.Code != 0)
                throw new BalanceOperationException("Unexpected cash out response status", result.Code);

            return result.TransactionId;
        }

        public async Task<string> CashOutAsync(string walletId, string assetId, decimal amount, string userId,
            string comment)
        {
            _log.InfoWithDetails("Cash out request", new {walletId, assetId, amount, userId, comment});

            ExchangeOperationResult result;

            try
            {
                result = await RetriesWrapper.RunWithRetriesAsync(() => _exchangeOperationsServiceClient
                    .ManualCashOutAsync(walletId, "empty", (double) amount, assetId, comment: comment, userId: userId));
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, new {walletId, assetId, amount, userId, comment});

                throw new ExchangeException("An error occurred while processing cash out request", exception);
            }

            _log.InfoWithDetails("Cash out response", result);

            if (result.Code != 0)
            {
                if(result.Code == 401)
                    throw new NotEnoughFundsException();
                
                throw new BalanceOperationException("Unexpected cash out response status", result.Code);
            }

            return result.TransactionId;
        }

        private static OrderAction ToOrderAction(LimitOrderType limitOrderType)
        {
            if (limitOrderType == LimitOrderType.Buy)
                return OrderAction.Buy;

            if (limitOrderType == LimitOrderType.Sell)
                return OrderAction.Sell;

            throw new InvalidEnumArgumentException(nameof(limitOrderType), (int) limitOrderType,
                typeof(LimitOrderType));
        }

        private static LimitOrderError ToLimitOrderError(MeStatusCodes meStatusCode)
        {
            switch (meStatusCode)
            {
                case MeStatusCodes.Ok:
                    return LimitOrderError.None;
                case MeStatusCodes.LowBalance:
                    return LimitOrderError.LowBalance;
                case MeStatusCodes.NoLiquidity:
                    return LimitOrderError.NoLiquidity;
                case MeStatusCodes.NotEnoughFunds:
                    return LimitOrderError.NotEnoughFunds;
                case MeStatusCodes.ReservedVolumeHigherThanBalance:
                    return LimitOrderError.ReservedVolumeHigherThanBalance;
                case MeStatusCodes.BalanceLowerThanReserved:
                    return LimitOrderError.BalanceLowerThanReserved;
                case MeStatusCodes.LeadToNegativeSpread:
                    return LimitOrderError.LeadToNegativeSpread;
                case MeStatusCodes.TooSmallVolume:
                    return LimitOrderError.TooSmallVolume;
                case MeStatusCodes.InvalidPrice:
                    return LimitOrderError.InvalidPrice;
                default:
                    return LimitOrderError.Unknown;
            }
        }
    }
}
