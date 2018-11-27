using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.IndexPrices;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with index prices.
    /// </summary>
    [PublicAPI]
    public interface IIndexPricesApi
    {
        /// <summary>
        /// Returns all index prices.
        /// </summary>
        /// <returns>s</returns>
        [Get("/api/IndexPrices")]
        Task<IReadOnlyCollection<IndexPriceModel>> GetAllAsync();

        /// <summary>
        /// Returns index price by index name.
        /// </summary>
        /// <param name="indexName">The name of index.</param>
        /// <returns>An index price.</returns>
        [Get("/api/IndexPrices/{indexName}")]
        Task<IndexPriceModel> GetByIndexAsync(string indexName);
    }
}
