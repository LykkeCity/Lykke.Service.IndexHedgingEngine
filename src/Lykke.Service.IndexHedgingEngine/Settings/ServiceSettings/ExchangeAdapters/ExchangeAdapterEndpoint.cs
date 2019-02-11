using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Settings.ServiceSettings.ExchangeAdapters
{
    [UsedImplicitly]
    public class ExchangeAdapterEndpoint
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string ApiKey { get; set; }
    }
}
