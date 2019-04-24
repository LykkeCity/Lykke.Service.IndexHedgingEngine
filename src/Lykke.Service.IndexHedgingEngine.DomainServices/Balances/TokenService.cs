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
using Lykke.Service.IndexHedgingEngine.Domain.Trades;
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
                Token previousToken = await GetAsync(assetId);
                Token currentToken = previousToken.Copy();

                string walletId = _settingsService.GetWalletId();

                string transactionId;

                switch (balanceOperationType)
                {
                    case BalanceOperationType.CashIn:
                        currentToken.IncreaseAmount(amount);
                        transactionId = await _lykkeExchangeService
                            .CashInAsync(walletId, assetId, amount, userId, comment);
                        break;

                    case BalanceOperationType.CashOut:
                        currentToken.DecreaseAmount(amount);
                        try
                        {
                            transactionId = await _lykkeExchangeService
                                .CashOutAsync(walletId, assetId, amount, userId, comment);
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

                await _tokenRepository.SaveAsync(currentToken);

                _cache.Set(currentToken);

                var balanceOperation = new BalanceOperation
                {
                    Timestamp = DateTime.UtcNow,
                    AssetId = assetId,
                    Type = balanceOperationType,
                    Amount = amount,
                    IsCredit = false,
                    Comment = comment,
                    UserId = userId,
                    TransactionId = transactionId
                };

                await _balanceOperationService.AddAsync(balanceOperation);

                _log.InfoWithDetails("Token amount updated", new
                {
                    PreviuosToken = previousToken,
                    CurrentToken = currentToken,
                    BalanceOperation = balanceOperation
                });
            }
            catch (Exception exception)
            {
                _log.WarningWithDetails("An error occurred while updating token", exception, new
                {
                    AssetId = assetId,
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

        public async Task UpdateVolumeAsync(string assetId, InternalTrade internalTrade)
        {
            await _semaphore.WaitAsync();

            try
            {
                Token previousToken = await GetAsync(assetId);
                Token currentToken = previousToken.Copy();

                if (internalTrade.Type == TradeType.Sell)
                    currentToken.IncreaseVolume(internalTrade.Volume, internalTrade.OppositeVolume);
                else
                    currentToken.DecreaseVolume(internalTrade.Volume, internalTrade.OppositeVolume);

                await _tokenRepository.SaveAsync(currentToken);

                _cache.Set(currentToken);

                _log.InfoWithDetails("Token open amount updated", new
                {
                    PreviuosToken = previousToken,
                    CurrentToken = currentToken,
                    InternalTrade = internalTrade
                });
            }
            catch (Exception exception)
            {
                _log.WarningWithDetails("An error occurred while updating open tokens", exception, new
                {
                    AssetId = assetId,
                    InternalTrade = internalTrade
                });

                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CloseAsync(string assetId, decimal volume, decimal price)
        {
            Token previousToken = await GetAsync(assetId);
            Token currentToken = previousToken.Copy();
            
            currentToken.Close(volume, price);
            
            await _tokenRepository.SaveAsync(currentToken);

            _cache.Set(currentToken);

            _log.InfoWithDetails("Token open amount updated", new
            {
                PreviuosToken = previousToken,
                CurrentToken = currentToken
            });
        }

        private static string GetKey(Token token)
            => GetKey(token.AssetId);

        private static string GetKey(string assetId)
            => assetId.ToUpper();
    }
}
