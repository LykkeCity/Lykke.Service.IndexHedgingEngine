using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Balances.AutorestClient.Models;
using Lykke.Service.Balances.Client;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;
using Lykke.Service.IndexHedgingEngine.DomainServices.Utils;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Balances
{
    [UsedImplicitly]
    public class BalanceService : IBalanceService
    {
        private readonly ISettingsService _settingsService;
        private readonly IBalancesClient _balancesClient;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly IBalanceOperationService _balanceOperationService;
        private readonly TraceWriter _traceWriter;
        private readonly InMemoryCache<Balance> _cache;
        private readonly ILog _log;

        public BalanceService(
            ISettingsService settingsService,
            IBalancesClient balancesClient,
            ILykkeExchangeService lykkeExchangeService,
            IBalanceOperationService balanceOperationService,
            TraceWriter traceWriter,
            ILogFactory logFactory)
        {
            _settingsService = settingsService;
            _balancesClient = balancesClient;
            _lykkeExchangeService = lykkeExchangeService;
            _balanceOperationService = balanceOperationService;
            _traceWriter = traceWriter;
            _cache = new InMemoryCache<Balance>(GetKey, true);
            _log = logFactory.CreateLog(this);
        }

        public IReadOnlyCollection<Balance> GetByExchange(string exchange)
        {
            return _cache.GetAll()
                .Where(o => o.Exchange == exchange)
                .ToArray();
        }

        public Balance GetByAssetId(string exchange, string assetId)
        {
            return _cache.Get(GetKey(exchange, assetId)) ?? new Balance(exchange, assetId, decimal.Zero, decimal.Zero);
        }

        public async Task UpdateAsync()
        {
            string walletId = _settingsService.GetWalletId();

            if (string.IsNullOrEmpty(walletId))
                return;

            try
            {
                IEnumerable<ClientBalanceResponseModel> response =
                    await _balancesClient.GetClientBalances(walletId);

                Balance[] balances = response
                    .Select(o => new Balance(ExchangeNames.Lykke, o.AssetId, o.Balance, o.Reserved))
                    .ToArray();

                _cache.Set(balances);

                _traceWriter.Balances(balances);
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, "An error occurred while getting balances from Lykke exchange.");
            }
        }

        public async Task UpdateAsync(string assetId, BalanceOperationType balanceOperationType, decimal amount,
            string comment, string userId)
        {
            try
            {
                string walletId = _settingsService.GetWalletId();

                string transactionId;

                switch (balanceOperationType)
                {
                    case BalanceOperationType.CashIn:
                        transactionId =
                            await _lykkeExchangeService.CashInAsync(walletId, assetId, amount, userId, comment);
                        break;

                    case BalanceOperationType.CashOut:
                        try
                        {
                            transactionId =
                                await _lykkeExchangeService.CashOutAsync(walletId, assetId, amount, userId, comment);
                        }
                        catch (NotEnoughFundsException)
                        {
                            throw new InvalidOperationException("No enough funds");
                        }

                        break;

                    default:
                        throw new InvalidEnumArgumentException(nameof(balanceOperationType), (int) balanceOperationType,
                            typeof(BalanceOperationType));
                }

                var balanceOperation = new BalanceOperation
                {
                    Timestamp = DateTime.UtcNow,
                    AssetId = assetId,
                    Type = balanceOperationType,
                    IsCredit = false,
                    Amount = amount,
                    Comment = comment,
                    UserId = userId,
                    TransactionId = transactionId
                };

                await _balanceOperationService.AddAsync(balanceOperation);

                _log.InfoWithDetails("Balance changed", balanceOperation);
            }
            catch (Exception exception)
            {
                _log.WarningWithDetails("An error occurred while changing balance", exception, new
                {
                    AssetId = assetId,
                    Type = balanceOperationType,
                    Amount = amount,
                    Comment = comment,
                    UserId = userId
                });

                throw;
            }
        }
        
        private static string GetKey(Balance balance)
            => GetKey(balance.Exchange, balance.AssetId);

        private static string GetKey(string exchange, string assetId)
            => $"{exchange}_{assetId}";
    }
}
