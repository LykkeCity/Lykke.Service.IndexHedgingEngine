using Lykke.Service.IndexHedgingEngine.Domain.Constants;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents an asset hedge settings.
    /// </summary>
    public class AssetHedgeSettings
    {
        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The name of hedge exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The identifier of the external asset pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The min volume that can be used to create external limit order.
        /// </summary>
        public decimal MinVolume { get; set; }

        /// <summary>
        /// The accuracy of the hedge limit order volume.
        /// </summary>
        public int VolumeAccuracy { get; set; }

        /// <summary>
        /// The accuracy of the hedge limit order price.
        /// </summary>
        public int PriceAccuracy { get; set; }

        /// <summary>
        /// Indicates that the asset hedge settings was approved after auto creation.
        /// </summary>
        public bool Approved { get; set; }

        /// <summary>
        /// Indicates that the asset hedge settings enabled.
        /// </summary>
        public bool Enabled { get; set; }

        public void Update(AssetHedgeSettings assetHedgeSettings)
        {
            Exchange = assetHedgeSettings.Exchange;
            AssetPairId = assetHedgeSettings.AssetPairId;
            MinVolume = assetHedgeSettings.MinVolume;
            VolumeAccuracy = assetHedgeSettings.VolumeAccuracy;
            PriceAccuracy = assetHedgeSettings.PriceAccuracy;
            Enabled = assetHedgeSettings.Enabled;
        }

        public void Approve()
        {
            Approved = true;
        }

        public static AssetHedgeSettings Create(string assetId)
        {
            return new AssetHedgeSettings
            {
                AssetId = assetId,
                Exchange = ExchangeNames.Virtual,
                AssetPairId = $"{assetId}USD",
                MinVolume = 0,
                VolumeAccuracy = 0,
                PriceAccuracy = 0,
                Approved = false,
                Enabled = false
            };
        }
    }
}
