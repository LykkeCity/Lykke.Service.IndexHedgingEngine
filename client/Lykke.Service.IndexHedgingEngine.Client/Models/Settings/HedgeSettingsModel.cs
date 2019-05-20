using System;
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
        [Obsolete]
        public decimal ThresholdUp { get; set; }

        /// <summary>
        /// The lower delta threshold used for risk exposure.
        /// </summary>
        [Obsolete]
        public decimal ThresholdDown { get; set; }

        /// <summary>
        /// The upper delta threshold used for risk exposure for buying.
        /// </summary>
        public decimal ThresholdUpBuy { get; set; }

        /// <summary>
        /// The upper delta threshold used for risk exposure for selling.
        /// </summary>
        public decimal ThresholdUpSell { get; set; }

        /// <summary>
        /// The lower delta threshold used for risk exposure for buying.
        /// </summary>
        public decimal ThresholdDownBuy { get; set; }

        /// <summary>
        /// The lower delta threshold used for risk exposure for selling.
        /// </summary>
        public decimal ThresholdDownSell { get; set; }

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
