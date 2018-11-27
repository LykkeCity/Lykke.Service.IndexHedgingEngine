using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Funding
{
    /// <summary>
    /// Represents a funding amount.
    /// </summary>
    [PublicAPI]
    public class FundingModel
    {
        /// <summary>
        /// The amount of funds.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
