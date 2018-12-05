using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Reports
{
    /// <summary>
    /// Represents a brief report for a token.
    /// </summary>
    [PublicAPI]
    public class TokenReportModel
    {
        /// <summary>
        /// The name of the token.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The identifier of the asset.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The price of the token.
        /// </summary>
        public decimal? Price { get; set; }
		
        /// <summary>
        /// The value of open volume.
        /// </summary>
        public decimal OpenVolume { get; set; }
		
        /// <summary>
        /// The collection of assets' weights.
        /// </summary>
        public IReadOnlyCollection<AssetWeightModel> Weights { get; set; }
    }
}
