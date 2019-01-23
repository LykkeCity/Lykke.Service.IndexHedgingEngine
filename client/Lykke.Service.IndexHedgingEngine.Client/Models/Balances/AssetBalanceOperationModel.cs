using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Balances
{
    /// <summary>
    /// Represent an asset balance operation.
    /// </summary>
    [PublicAPI]
    public class AssetBalanceOperationModel
    {
        /// <summary>
        /// The identifier of asset.
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
    }
}
