using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settings
{
    /// <summary>
    /// Represents a hedge algorithm settings.
    /// </summary>
    [PublicAPI]
    public class HedgeSettingsModel
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
        /// The markup of market hedge limit order.
        /// </summary>
        public decimal MarketOrderMarkup { get; set; }
    }
}
