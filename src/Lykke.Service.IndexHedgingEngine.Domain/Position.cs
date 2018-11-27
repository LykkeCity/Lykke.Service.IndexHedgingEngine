namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents an opened position by asset. 
    /// </summary>
    public class Position
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

        public void Increase(decimal volume, decimal oppositeVolume)
        {
            Volume += volume;

            OppositeVolume -= oppositeVolume;
        }

        public void Decrease(decimal volume, decimal oppositeVolume)
        {
            Volume -= volume;

            OppositeVolume += oppositeVolume;
        }

        public static Position Create(string assetId, string exchange, decimal volume, decimal oppositeVolume)
        {
            return new Position
            {
                AssetId = assetId,
                Exchange = exchange,
                Volume = volume,
                OppositeVolume = oppositeVolume
            };
        }

        public void Close(decimal price)
        {
            OppositeVolume += Volume * price;
            
            Volume = 0;
        }
    }
}
