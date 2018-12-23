using System;
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
        [Obsolete]
        public decimal MinVolume { get; set; }

        /// <summary>
        /// The accuracy of the hedge limit order volume.
        /// </summary>
        [Obsolete]
        public int VolumeAccuracy { get; set; }

        /// <summary>
        /// The accuracy of the hedge limit order price.
        /// </summary>
        [Obsolete]
        public int PriceAccuracy { get; set; }

        /// <summary>
        /// Returns <c>false</c> if asset hedge settings created automatically and indicates that the user should update default settings.
        /// </summary>
        public bool Approved { get; set; }

        /// <summary>
        /// The asset hedging mode.
        /// </summary>
        public AssetHedgeMode Mode { get; set; }

        public void Update(AssetHedgeSettings assetHedgeSettings)
        {
            Exchange = assetHedgeSettings.Exchange;
            AssetPairId = assetHedgeSettings.AssetPairId;
            MinVolume = assetHedgeSettings.MinVolume;
            VolumeAccuracy = assetHedgeSettings.VolumeAccuracy;
            PriceAccuracy = assetHedgeSettings.PriceAccuracy;
            Mode = assetHedgeSettings.Mode;
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
                Mode = AssetHedgeMode.Disabled
            };
        }
    }
}
