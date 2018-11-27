using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.HedgeLimitOrders;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Reports
{
    /// <summary>
    /// Represents an opened position by asset. 
    /// </summary>
    [PublicAPI]
    public class PositionReportModel
    {
        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The current price of asset.
        /// </summary>
        public QuoteModel Quote { get; set; }

        /// <summary>
        /// The current volume of position.
        /// </summary>
        public decimal? Volume { get; set; }

        /// <summary>
        /// The current amount of volume in USD.
        /// </summary>
        public decimal? VolumeInUsd { get; set; }
        
        /// <summary>
        /// The current opposite volume of position (amount of USD spend to open position).
        /// </summary>
        public decimal? OppositeVolume { get; set; }

        /// <summary>
        /// The deference between <see cref="OppositeVolume"/> and <see cref="VolumeInUsd"/>.
        /// </summary>
        public decimal? PnL { get; set; }

        /// <summary>
        /// The current hedge limit order that associated with asset.
        /// </summary>
        public HedgeLimitOrderModel HedgeLimitOrder { get; set; }

        /// <summary>
        /// The current asset investments that associated with position.
        /// </summary>
        public AssetInvestmentModel AssetInvestment { get; set; }
        
        /// <summary>
        /// The details if an error that occurred during calculating position.
        /// </summary>
        public string Error { get; set; }
    }
}
