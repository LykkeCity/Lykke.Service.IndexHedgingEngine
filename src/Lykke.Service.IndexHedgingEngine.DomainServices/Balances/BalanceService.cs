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
using Lykke.Service.IndexHedgingEngine.DomainServices.Utils;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Balances
{
    [UsedImplicitly]
    public class BalanceService : IBalanceService
    {
        private readonly ISettingsService _settingsService;
        private readonly IBalancesClient _balancesClient;
        private readonly TraceWriter _traceWriter;
        private readonly InMemoryCache<Balance> _cache;
        private readonly ILog _log;

        public BalanceService(
            ISettingsService settingsService,
            IBalancesClient balancesClient,
            TraceWriter traceWriter,
            ILogFactory logFactory)
        {
            _settingsService = settingsService;
            _balancesClient = balancesClient;
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

        private static string GetKey(Balance balance)
            => GetKey(balance.Exchange, balance.AssetId);

        private static string GetKey(string exchange, string assetId)
            => $"{exchange}_{assetId}";
    }
}
