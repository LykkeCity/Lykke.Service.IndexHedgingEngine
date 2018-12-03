using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    public class TokenService : ITokenService
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly ITokenRepository _tokenRepository;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly ISettingsService _settingsService;
        private readonly IBalanceOperationService _balanceOperationService;

        private readonly ILog _log;
        private readonly InMemoryCache<Token> _cache;

        public TokenService(
            ITokenRepository tokenRepository,
            ILykkeExchangeService lykkeExchangeService,
            ISettingsService settingsService,
            IBalanceOperationService balanceOperationService,
            ILogFactory logFactory)
        {
            _tokenRepository = tokenRepository;
            _lykkeExchangeService = lykkeExchangeService;
            _settingsService = settingsService;
            _balanceOperationService = balanceOperationService;

            _cache = new InMemoryCache<Token>(GetKey, false);
            _log = logFactory.CreateLog(this);
        }

        public async Task<IReadOnlyCollection<Token>> GetAllAsync()
        {
            IReadOnlyCollection<Token> tokens = _cache.GetAll();

            if (tokens == null)
            {
                tokens = await _tokenRepository.GetAllAsync();

                _cache.Initialize(tokens);
            }

            return tokens;
        }

        public async Task<Token> GetAsync(string assetId)
        {
            IReadOnlyCollection<Token> tokens = await GetAllAsync();

            return tokens.SingleOrDefault(o => o.AssetId == assetId) ?? new Token {AssetId = assetId};
        }

        public async Task UpdateAmountAsync(string assetId, BalanceOperationType balanceOperationType, decimal amount,
            string comment, string userId)
        {
            await _semaphore.WaitAsync();

            try
            {
                Token token = (await GetAsync(assetId)).Copy();

                string walletId = await _settingsService.GetWalletIdAsync();

                string transactionId;

                switch (balanceOperationType)
                {
                    case BalanceOperationType.CashIn:
                        token.IncreaseAmount(amount);
                        transactionId =
                            await _lykkeExchangeService.CashInAsync(walletId, assetId, amount, userId, comment);
                        break;

                    case BalanceOperationType.CashOut:
                        token.DecreaseAmount(amount);
                        try
                        {
                            transactionId =
                                await _lykkeExchangeService.CashOutAsync(walletId, assetId, amount, userId, comment);
                        }
                        catch (BalanceOperationException exception) when (exception.Code == 401)
                        {
                            throw new InvalidOperationException("No enough funds");
                        }

                        break;

                    default:
                        throw new InvalidEnumArgumentException(nameof(balanceOperationType), (int) balanceOperationType,
                            typeof(BalanceOperationType));
                }

                await _tokenRepository.InsertOrReplaceAsync(token);

                _cache.Set(token);

                var balanceOperation = new BalanceOperation
                {
                    Timestamp = DateTime.UtcNow,
                    AssetId = assetId,
                    Type = balanceOperationType,
                    Amount = amount,
                    Comment = comment,
                    UserId = userId,
                    TransactionId = transactionId
                };

                await _balanceOperationService.AddAsync(balanceOperation);

                _log.InfoWithDetails("Token amount updated", balanceOperation);
            }
            catch (Exception exception)
            {
                _log.WarningWithDetails("An error occurred while updating token", exception, new
                {
                    assetId,
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

        public async Task UpdateVolumeAsync(string assetId, TradeType tradeType, decimal volume,
            decimal oppositeVolume)
        {
            await _semaphore.WaitAsync();

            try
            {
                Token token = (await GetAsync(assetId)).Copy();
                
                if (tradeType == TradeType.Sell)
                    token.IncreaseVolume(volume, oppositeVolume);
                else
                    token.DecreaseVolume(volume, oppositeVolume);
                
                await _tokenRepository.InsertOrReplaceAsync(token);

                _cache.Set(token);

                _log.InfoWithDetails("Token open amount updated", token);
            }
            catch (Exception exception)
            {
                _log.WarningWithDetails("An error occurred while updating open tokens", exception, new
                {
                    assetId,
                    tradeType,
                    volume,
                    oppositeVolume
                });

                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static string GetKey(Token token)
            => GetKey(token.AssetId);

        private static string GetKey(string assetId)
            => assetId.ToUpper();
    }
}
