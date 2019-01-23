using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Balances
{
    [UsedImplicitly]
    public class FundingService : IFundingService
    {
        private const string CacheKey = "Key";
        private const string AssetId = "USD";

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly IFundingRepository _fundingRepository;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly ISettingsService _settingsService;
        private readonly IBalanceOperationService _balanceOperationService;
        private readonly ILog _log;
        private readonly InMemoryCache<Funding> _cache;

        public FundingService(
            IFundingRepository fundingRepository,
            ILykkeExchangeService lykkeExchangeService,
            ISettingsService settingsService,
            IBalanceOperationService balanceOperationService,
            ILogFactory logFactory)
        {
            _fundingRepository = fundingRepository;
            _lykkeExchangeService = lykkeExchangeService;
            _settingsService = settingsService;
            _balanceOperationService = balanceOperationService;
            _cache = new InMemoryCache<Funding>(funding => CacheKey, false);
            _log = logFactory.CreateLog(this);
        }

        public async Task<Funding> GetAsync()
        {
            Funding funding = _cache.Get(CacheKey);

            if (funding == null)
            {
                funding = await _fundingRepository.GetAsync();

                if (funding == null)
                    funding = new Funding();

                _cache.Initialize(new[] {funding});
            }

            return funding;
        }

        public async Task UpdateAsync(BalanceOperationType balanceOperationType, decimal amount, string comment,
            string userId)
        {
            await _semaphore.WaitAsync();

            try
            {
                Funding funding = (await GetAsync()).Copy();

                string walletId = _settingsService.GetWalletId();

                string transactionId;

                switch (balanceOperationType)
                {
                    case BalanceOperationType.CashIn:
                        funding.Add(amount);
                        transactionId =
                            await _lykkeExchangeService.CashInAsync(walletId, AssetId, amount, userId, comment);
                        break;

                    case BalanceOperationType.CashOut:
                        funding.Subtract(amount);
                        try
                        {
                            transactionId =
                                await _lykkeExchangeService.CashOutAsync(walletId, AssetId, amount, userId, comment);
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

                await _fundingRepository.InsertOrReplaceAsync(funding);

                _cache.Set(funding);

                var balanceOperation = new BalanceOperation
                {
                    Timestamp = DateTime.UtcNow,
                    AssetId = AssetId,
                    Type = balanceOperationType,
                    IsCredit = true,
                    Amount = amount,
                    Comment = comment,
                    UserId = userId,
                    TransactionId = transactionId
                };

                await _balanceOperationService.AddAsync(balanceOperation);

                _log.InfoWithDetails("Funding amount updated", balanceOperation);
            }
            catch (Exception exception)
            {
                _log.WarningWithDetails("An error occurred while updating funding", exception, new
                {
                    AssetId,
                    Type = balanceOperationType,
                    Amount = amount,
                    Comment = comment,
                    UserId = userId
                });

                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
