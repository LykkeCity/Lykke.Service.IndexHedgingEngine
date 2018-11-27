using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Tokens
{
    /// <summary>
    /// Represents an amount of produced tokens.
    /// </summary>
    [PublicAPI]
    public class TokenModel
    {
        /// <summary>
        /// The identifier of the asset that associated with token.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The amount of produced tokens.
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// The amount of tokens that sold to the clients.
        /// </summary>
        public decimal OpenVolume { get; set; }
        
        /// <summary>
        /// The amount of USD that received from clients.
        /// </summary>
        public decimal OppositeVolume { get; set; }
    }
}
