using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settings
{
    [UsedImplicitly]
    public class SettingsService : ISettingsService
    {
        private readonly string _instanceName;
        private readonly string _walletId;
        private readonly string _transitWalletId;
        private readonly IReadOnlyCollection<ExchangeSettings> _exchanges;

        public SettingsService(
            string instanceName,
            string walletId,
            string transitWalletId,
            IReadOnlyCollection<string> exchangeAdapters)
        {
            _instanceName = instanceName;
            _walletId = walletId;
            _transitWalletId = transitWalletId;
            _exchanges = new[]
                {
                    new ExchangeSettings
                    {
                        Name = ExchangeNames.Lykke,
                        Fee = decimal.Zero,
                        HasApi = true
                    },
                    new ExchangeSettings
                    {
                        Name = ExchangeNames.Virtual,
                        Fee = decimal.Zero,
                        HasApi = true
                    }
                }
                .Union(exchangeAdapters.Select(o =>
                    new ExchangeSettings
                    {
                        Name = o,
                        Fee = decimal.Zero,
                        HasApi = true
                    }))
                .ToArray();
        }

        public string GetInstanceName()
            => _instanceName;

        public string GetWalletId()
            => _walletId;

        public string GetTransitWalletId()
            => _transitWalletId;

        public IReadOnlyCollection<ExchangeSettings> GetExchanges()
            => _exchanges;
    }
}
