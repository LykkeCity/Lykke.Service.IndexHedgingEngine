using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Tokens;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with tokens.
    /// </summary>
    [PublicAPI]
    public interface ITokensApi
    {
        /// <summary>
        /// Returns a collection of tokens amount.
        /// </summary>
        /// <returns>A collection of tokens amount.</returns>
        [Get("/api/Tokens")]
        Task<IReadOnlyCollection<TokenModel>> GetAllAsync();

        /// <summary>
        /// Updates token amount.
        /// </summary>
        /// <param name="model">The model that represents token operation.</param>
        [Post("/api/Tokens")]
        Task UpdateAsync([Body] TokenOperationModel model);
    }
}
