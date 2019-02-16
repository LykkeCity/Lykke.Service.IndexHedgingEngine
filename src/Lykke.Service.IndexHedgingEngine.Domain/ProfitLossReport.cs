namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represent profit loss report.
    /// </summary>
    public class ProfitLossReport
    {
        public ProfitLossReport()
        {
        }
        
        public ProfitLossReport(decimal balance, decimal pnL)
        {
            Balance = balance;
            PnL = pnL;
        }

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
