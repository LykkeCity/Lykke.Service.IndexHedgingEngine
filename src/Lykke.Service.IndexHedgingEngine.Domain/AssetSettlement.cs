namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represent an asset settlement.
    /// </summary>
    public class AssetSettlement
    {
        /// <summary>
        /// The identifier of the settlement.
        /// </summary>
        public string SettlementId { get; set; }

        /// <summary>
        /// The identifier of the asset to settle.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The amount of asset to settle.
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// The price in USD of the asset that was while requesting the settlement.
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// The fee of settlement asset on external exchange, zero for Lykke exchange.
        /// </summary>
        public decimal Fee { get; set; }
        
        /// <summary>
        /// The weight of the asset in index that was while requesting the settlement.
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// If <c>true</c> the asset should be settle direct, otherwise in USD. 
        /// </summary>
        public bool IsDirect { get; set; }
        
        /// <summary>
        /// If <c>true</c> the asset is on an external exchange, otherwise on Lykke exchange. 
        /// </summary>
        public bool IsExternal { get; set; }
        
        /// <summary>
        /// The status of the asset settlement.
        /// </summary>
        public AssetSettlementStatus Status { get; set; }

        /// <summary>
        /// The details of the error that occurred while processing the asset settlement.
        /// </summary>
        public SettlementError Error { get; set; }

        /// <summary>
        /// The actual amount of the asset. Can be different for external asset.
        /// </summary>
        public decimal ActualAmount { get; set; }
        
        /// <summary>
        /// The actual price of the asset. Can be different for external asset.
        /// </summary>
        public decimal ActualPrice { get; set; }
        
        /// <summary>
        /// The identifier of the client wallet cash-in transaction.
        /// </summary>
        public string TransactionId { get; set; }

        public bool IsManual()
            => IsDirect && IsExternal;
        
        public void Update(decimal amount, bool isDirect, bool isExternal)
        {
            Amount = amount;
            IsDirect = isDirect;
            IsExternal = isExternal;
            ActualAmount = amount;
        }
    }
}
