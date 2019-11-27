using System;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents a hedge algorithm settings.
    /// </summary>
    public class HedgeSettings
    {
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
