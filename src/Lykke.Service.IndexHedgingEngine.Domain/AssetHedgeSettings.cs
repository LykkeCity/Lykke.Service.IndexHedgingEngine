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
        /// Returns <c>false</c> if asset hedge settings created automatically and indicates that the user should update default settings.
        /// </summary>
        public bool Approved { get; set; }

        /// <summary>
        /// The asset hedging mode.
        /// </summary>
        public AssetHedgeMode Mode { get; set; }

        /// <summary>
        /// The name of the exchange that used to control deviation of hedge limit order price.
        /// </summary>
        public string ReferenceExchange { get; set; }
        
        /// <summary>
        /// The allowed deviation of hedge limit order price in comparison with <see cref="ReferenceExchange"/>.
        /// </summary>
        public decimal? ReferenceDelta { get; set; }
        
        public void Update(AssetHedgeSettings assetHedgeSettings)
        {
            Exchange = assetHedgeSettings.Exchange;
            AssetPairId = assetHedgeSettings.AssetPairId;
            Mode = assetHedgeSettings.Mode;
            ReferenceExchange = assetHedgeSettings.ReferenceExchange;
            ReferenceDelta = assetHedgeSettings.ReferenceDelta;
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
                Approved = false,
                Mode = AssetHedgeMode.Disabled
            };
        }
    }
}
