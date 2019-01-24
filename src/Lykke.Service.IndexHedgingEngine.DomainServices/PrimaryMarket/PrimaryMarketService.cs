using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.Balances.Client;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.PrimaryMarket
{
    [UsedImplicitly]
    public class PrimaryMarketService : IPrimaryMarketService
    {
        private readonly string _walletId;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly IBalancesClient _balancesClient;
        private readonly IPrimaryMarketBalanceUpdatesRepository _primaryMarketRepository;

        public PrimaryMarketService(
            string walletId,
            ILykkeExchangeService lykkeExchangeService,
            IBalancesClient balancesClient,
            IPrimaryMarketBalanceUpdatesRepository primaryMarketRepository)
        {
            _walletId = walletId;
            _lykkeExchangeService = lykkeExchangeService;
            _balancesClient = balancesClient;
            _primaryMarketRepository = primaryMarketRepository;
        }
        
        public Task<string> GetPrimaryMarketWalletIdAsync()
        {
            return Task.FromResult(_walletId);
        }

        public async Task<IReadOnlyList<PrimaryMarketBalance>> GetBalancesAsync()
        {
            var balances = await _balancesClient.GetClientBalances(_walletId);

            return balances
                .Select(x => new PrimaryMarketBalance
                {
                    AssetId = x.AssetId,
                    Balance = x.Balance,
                    Reserved = x.Reserved
                })
                .Where(x => x.Balance != 0 || x.Reserved != 0)
                .ToArray();
        }

        public async Task UpdateBalanceAsync(string assetId, decimal amount, string userId, string comment)
        {
            if (amount > 0)
            {
                await _lykkeExchangeService.CashInAsync(
                    _walletId,
                    assetId,
                    amount,
                    userId,
                    comment);
            }
            else if (amount < 0)
            {
                await _lykkeExchangeService.CashOutAsync(
                    _walletId,
                    assetId,
                    Math.Abs(amount),
                    userId,
                    comment);
            }

            await _primaryMarketRepository.CreateAsync(new PrimaryMarketHistoryItem
            {
                AssetId = assetId,
                Amount = amount,
                DateTime = DateTime.UtcNow,
                UserId = userId,
                Comment = comment
            });
        }

        public Task<IReadOnlyList<PrimaryMarketHistoryItem>> GetHistoryAsync()
        {
            return _primaryMarketRepository.GetItemsAsync();
        }
    }
}
