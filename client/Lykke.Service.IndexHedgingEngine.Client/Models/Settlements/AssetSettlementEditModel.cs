using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Settlements
{
    /// <summary>
    /// Represents the asset settlement edit information. 
    /// </summary>
    [PublicAPI]
    public class AssetSettlementEditModel
    {
        /// <summary>
        /// The identifier of the asset to settle.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The amount of asset to settle.
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// If <c>true</c> the asset should be settle direct, otherwise in USD.
        /// </summary>
        public bool IsDirect { get; set; }
        
        /// <summary>
        /// If <c>true</c> the asset is on an external exchange, otherwise on Lykke exchange.
        /// </summary>
        public bool IsExternal { get; set; }
    }
}
