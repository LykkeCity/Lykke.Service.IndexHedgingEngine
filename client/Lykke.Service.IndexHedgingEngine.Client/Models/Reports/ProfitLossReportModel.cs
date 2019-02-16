using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Reports
{
    /// <summary>
    /// Represent profit loss report.
    /// </summary>
    [PublicAPI]
    public class ProfitLossReportModel
    {
        /// <summary>
        /// The amount of current balance in USD.
        /// </summary>
        public decimal Balance { get; set; }
        
        /// <summary>
        /// The amount of profit loss. 
        /// </summary>
        public decimal PnL { get; set; }
    }
}
