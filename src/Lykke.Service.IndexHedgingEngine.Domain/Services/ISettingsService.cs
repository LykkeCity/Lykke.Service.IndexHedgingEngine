using System.Collections.Generic;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ISettingsService
    {
        string GetInstanceName();

        string GetWalletId();

        string GetTransitWalletId();

        IReadOnlyCollection<ExchangeSettings> GetExchanges();
    }
}
