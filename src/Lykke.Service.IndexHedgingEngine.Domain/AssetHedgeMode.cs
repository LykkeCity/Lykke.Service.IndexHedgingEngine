namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Specifies an asset hedging mode.
    /// </summary>
    public enum AssetHedgeMode
    {
        /// <summary>
        /// Unspecified.
        /// </summary>
        None,
        
        /// <summary>
        /// Indicates that the hedging for the asset is not allowed.
        /// </summary>
        Disabled,
        
        /// <summary>
        /// Indicates that the hedging for the asset is not allowed and a hedge limit order calculate but not executed.
        /// </summary>
        Idle,
        
        /// <summary>
        /// Indicates that the hedging for the asset allowed in manual manner.
        /// </summary>
        Manual,
        
        /// <summary>
        /// Indicates that the hedging for the asset is automatically processed by algorithm.
        /// </summary>
        Auto
    }
}
