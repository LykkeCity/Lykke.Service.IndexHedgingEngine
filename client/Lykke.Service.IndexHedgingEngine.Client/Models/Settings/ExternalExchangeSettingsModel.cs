using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settings
{
    /// <summary>
    /// Represent an exchange settings.
    /// </summary>
    [PublicAPI]
    public class ExchangeSettingsModel
    {
        /// <summary>
        /// The name of exchange.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The amount of fee on exchange.
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Exchange has adapter with trading api and can be used for hedging.
        /// </summary>
        public bool HasApi { get; set; }
    }
}
