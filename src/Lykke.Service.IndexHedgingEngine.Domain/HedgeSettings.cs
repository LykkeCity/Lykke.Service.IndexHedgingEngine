namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents a hedge algorithm settings.
    /// </summary>
    public class HedgeSettings
    {
        /// <summary>
        /// The upper delta threshold used for risk exposure.
        /// </summary>
        public decimal ThresholdUp { get; set; }

        /// <summary>
        /// The lower delta threshold used for risk exposure.
        /// </summary>
        public decimal ThresholdDown { get; set; }

        /// <summary>
        /// The critical delta threshold used to stop hedging.
        /// </summary>
        public decimal ThresholdCritical { get; set; }
        
        /// <summary>
        /// The markup of market hedge limit order.
        /// </summary>
        public decimal MarketOrderMarkup { get; set; }
    }
}
