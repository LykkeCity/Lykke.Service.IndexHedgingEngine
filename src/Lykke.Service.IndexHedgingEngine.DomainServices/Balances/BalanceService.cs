using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Balances.AutorestClient.Models;
using Lykke.Service.Balances.Client;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Balances
{
    [UsedImplicitly]
    public class BalanceService : IBalanceService
    {
        private readonly ISettingsService _settingsService;
        private readonly IBalancesClient _balancesClient;
        private readonly InMemoryCache<Balance> _cache;
        private readonly ILog _log;

        public BalanceService(
            ISettingsService settingsService,
            IBalancesClient balancesClient,
            ILogFactory logFactory)
        {
            _settingsService = settingsService;
            _balancesClient = balancesClient;
            _cache = new InMemoryCache<Balance>(GetKey, true);
            _log = logFactory.CreateLog(this);
        }

        public Task<IReadOnlyCollection<Balance>> GetAsync(string exchange)
        {
            IReadOnlyCollection<Balance> balances = _cache.GetAll()
                .Where(o => o.Exchange == exchange)
                .ToArray();

            return Task.FromResult(balances);
        }

        public Task<Balance> GetByAssetIdAsync(string exchange, string assetId)
        {
            Balance balance = _cache.Get(GetKey(exchange, assetId)) ??
                              new Balance(exchange, assetId, decimal.Zero, decimal.Zero);

            return Task.FromResult(balance);
        }

        public async Task UpdateAsync()
        {
            string walletId = _settingsService.GetWalletId();

            if (string.IsNullOrEmpty(walletId))
                return;

            try
            {
                IEnumerable<ClientBalanceResponseModel> balances =
                    await _balancesClient.GetClientBalances(walletId);

                _cache.Set(balances.Select(o => new Balance(ExchangeNames.Lykke, o.AssetId, o.Balance, o.Reserved))
                    .ToArray());
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, "An error occurred while getting balances from Lykke exchange.");
            }
        }

        private static string GetKey(Balance balance)
            => GetKey(balance.Exchange, balance.AssetId);

        private static string GetKey(string exchange, string assetId)
            => $"{exchange}_{assetId}";
    }
}
