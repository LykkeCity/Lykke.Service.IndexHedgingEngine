using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.Db
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DbSettings
    {
        [AzureTableCheck]
        public string DataConnectionString { get; set; }

        [AzureTableCheck]
        public string LogsConnectionString { get; set; }

        [AzureTableCheck]
        public string LykkeTradesMeQueuesDeduplicatorConnectionString { get; set; }

        [AzureTableCheck]
        public string LykkeHedgeTradesMeQueuesDeduplicatorConnectionString { get; set; }
    }
}
