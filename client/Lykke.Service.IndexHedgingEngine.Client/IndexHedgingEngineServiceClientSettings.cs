using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.IndexHedgingEngine.Client
{
    /// <summary>
    /// Index hedging engine client settings.
    /// </summary>
    [PublicAPI]
    public class IndexHedgingEngineServiceClientSettings
    {
        /// <summary>
        /// Service url.
        /// </summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
