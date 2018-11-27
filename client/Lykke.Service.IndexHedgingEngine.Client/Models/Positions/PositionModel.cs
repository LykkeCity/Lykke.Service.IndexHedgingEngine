using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Positions
{
    /// <summary>
    /// Represents an opened position by asset. 
    /// </summary>
    [PublicAPI]
    public class PositionModel
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
        /// The current volume of position.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// The current opposite volume of position (amount of USD spend to open position).
        /// </summary>
        public decimal OppositeVolume { get; set; }

        /// <summary>
        /// The current USD rate.
        /// </summary>
        public decimal? Rate { get; set; }

        /// <summary>
        /// The current volume in USD.
        /// </summary>
        public decimal? VolumeInUsd { get; set; }

        /// <summary>
        /// The deference between <see cref="OppositeVolume"/> and <see cref="VolumeInUsd"/>.
        /// </summary>
        public decimal? PnL { get; set; }
    }
}
