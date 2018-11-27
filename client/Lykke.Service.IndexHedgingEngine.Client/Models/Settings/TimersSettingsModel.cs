using System;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settings
{
    /// <summary>
    /// Represents a settings of timers.
    /// </summary>
    [PublicAPI]
    public class TimersSettingsModel
    {
        /// <summary>
        /// The timer interval of Lykke exchange balances.
        /// </summary>
        public TimeSpan LykkeBalances { get; set; }
        
        /// <summary>
        /// The timer interval of external exchange balances.
        /// </summary>
        public TimeSpan ExternalBalances { get; set; }
    }
}
