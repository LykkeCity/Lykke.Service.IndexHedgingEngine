using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Funding;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with finding.
    /// </summary>
    [PublicAPI]
    public interface IFundingApi
    {
        /// <summary>
        /// Returns a funding amount.
        /// </summary>
        /// <returns>The model that represents funding amount.</returns>
        [Get("/api/Funding")]
        Task<FundingModel> GetAllAsync();

        /// <summary>
        /// Updates funding amount.
        /// </summary>
        /// <param name="model">The model that represents funding operation.</param>
        [Post("/api/Funding")]
        Task UpdateAsync([Body] FundingOperationModel model);
    }
}
