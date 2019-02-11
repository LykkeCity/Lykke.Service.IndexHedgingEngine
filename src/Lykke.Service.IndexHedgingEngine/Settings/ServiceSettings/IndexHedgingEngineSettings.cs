using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.Db;
using Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.ExchangeAdapters;
using Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.Rabbit;

namespace Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IndexHedgingEngineSettings
    {
        public string Name { get; set; }

        public string WalletId { get; set; }

        public string TransitWalletId { get; set; }

        public string PrimaryMarketWalletId { get; set; }

        public DbSettings Db { get; set; }

        public RabbitSettings Rabbit { get; set; }

        public IReadOnlyCollection<ExchangeAdapterEndpoint> ExchangeAdapters { get; set; }
    }
}
