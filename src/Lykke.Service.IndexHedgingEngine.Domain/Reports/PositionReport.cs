namespace Lykke.Service.IndexHedgingEngine.Domain.Reports
{
    /// <summary>
    /// Represents an opened position by asset. 
    /// </summary>
    public class PositionReport
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
        public Quote Quote { get; set; }

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
        /// The deference between <see cref="OppositeVolume"/> and <see cref="VolumeInUsd"/> inverse sing for virtual exchange.
        /// </summary>
        public decimal? ActualPnL { get; set; }

        /// <summary>
        /// The current hedge limit order that associated with asset.
        /// </summary>
        public HedgeLimitOrder HedgeLimitOrder { get; set; }

        /// <summary>
        /// The current asset investments that associated with position.
        /// </summary>
        public AssetInvestment AssetInvestment { get; set; }
        
        /// <summary>
        /// The details if an error that occurred during calculating position.
        /// </summary>
        public string Error { get; set; }
    }
}
