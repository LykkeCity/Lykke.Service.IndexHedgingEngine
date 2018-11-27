using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IReadOnlyCollection<ExchangeSettings> _exchanges;

        public SettingsService(
            string instanceName,
            string walletId,
            IReadOnlyCollection<ExchangeSettings> exchanges)
        {
            _instanceName = instanceName;
            _walletId = walletId;
            // TODO: Union exchanges
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
            };
        }

        public Task<string> GetInstanceNameAsync()
        {
            return Task.FromResult(_instanceName);
        }

        public Task<string> GetWalletIdAsync()
        {
            return Task.FromResult(_walletId);
        }

        public Task<IReadOnlyCollection<ExchangeSettings>> GetExchangesAsync()
        {
            return Task.FromResult(_exchanges);
        }
    }
}
