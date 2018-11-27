using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Audit;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Tokens
{
    /// <summary>
    /// Represent a token operation details.
    /// </summary>
    [PublicAPI]
    public class TokenOperationModel
    {
        /// <summary>
        /// The identifier of the asset that associated with token.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The operation type.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public BalanceOperationType Type { get; set; }

        /// <summary>
        /// The amount of operation.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The comment that describes the operation.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The identifier of the user that instantiated operation.
        /// </summary>
        public string UserId { get; set; }
    }
}
