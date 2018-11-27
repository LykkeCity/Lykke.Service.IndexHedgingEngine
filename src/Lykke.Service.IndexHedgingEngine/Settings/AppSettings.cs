using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Assets.Client;
using Lykke.Service.Balances.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.IndexHedgingEngine.Settings.Clients.MatchingEngine;
using Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings;

namespace Lykke.Service.IndexHedgingEngine.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public IndexHedgingEngineSettings IndexHedgingEngineService { get; set; }

        public AssetServiceSettings AssetsServiceClient { get; set; }

        public BalancesServiceClientSettings BalancesServiceClient { get; set; }

        public ExchangeOperationsServiceClientSettings ExchangeOperationsServiceClient { get; set; }
        
        public MatchingEngineClientSettings MatchingEngineClient { get; set; }
    }
}
