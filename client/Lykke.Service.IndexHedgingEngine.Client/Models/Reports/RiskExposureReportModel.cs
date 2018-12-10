using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Reports
{
    /// <summary>
    /// Represents a risk exposure report.
    /// </summary>
    [PublicAPI]
    public class RiskExposureReportModel
    {
        /// <summary>
        /// The volume of remaining USD. 
        /// </summary>
        public decimal UsdCash { get; set; }
        
        /// <summary>
        /// The summary report data by tokens.
        /// </summary>
        public IReadOnlyCollection<TokenReportModel> Indices { get; set; }
        
        /// <summary>
        /// The delta summary by tokens.
        /// </summary>
        public IReadOnlyCollection<InstrumentDeltaReportModel> Tokens { get; set; }
	
        /// <summary>
        /// The delta summary by assets.
        /// </summary>
        public IReadOnlyCollection<InstrumentDeltaReportModel> Assets { get; set; }
    }
}
