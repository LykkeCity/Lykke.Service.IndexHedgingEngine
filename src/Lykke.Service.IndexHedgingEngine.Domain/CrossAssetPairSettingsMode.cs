using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents cross asset pairs setting mode.
    /// </summary>
    public enum CrossAssetPairSettingsMode
    {
        /// <summary>
        /// Unspecified mode.
        /// </summary>
        None,

        /// <summary>
        /// Cross asset pairs is used to calculate its orders.
        /// </summary>
        Enabled,

        /// <summary>
        /// Cross asset pairs is not used to calculate its orders.
        /// </summary>
        Disabled
    }
}
