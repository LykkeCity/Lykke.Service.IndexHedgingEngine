using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Positions
{
    /// <summary>
    /// Represents an operation to close position.
    /// </summary>
    [PublicAPI]
    public class ClosePositionOperationModel
    {
        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The name of the exchange.
        /// </summary>
        public string Exchange { get; set; }
    }
}
