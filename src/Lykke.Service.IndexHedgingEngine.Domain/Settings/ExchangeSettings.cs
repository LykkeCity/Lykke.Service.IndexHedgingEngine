namespace Lykke.Service.IndexHedgingEngine.Domain.Settings
{
    /// <summary>
    /// Represent an exchange settings.
    /// </summary>
    public class ExchangeSettings
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
