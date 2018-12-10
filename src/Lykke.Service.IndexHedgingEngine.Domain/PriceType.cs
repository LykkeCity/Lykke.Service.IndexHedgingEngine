namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Specifies a limit order price type.
    /// </summary>
    public enum PriceType
    {
        /// <summary>
        /// Unspecified price type.
        /// </summary>
        None,
        
        /// <summary>
        /// Best level price, can contains markups.
        /// </summary>
        Limit,
        
        /// <summary>
        /// Market price, can contains markups.
        /// </summary>
        Market
    }
}
