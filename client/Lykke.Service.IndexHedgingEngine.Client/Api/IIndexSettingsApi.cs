using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.IndexSettings;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with index settings.
    /// </summary>
    [PublicAPI]
    public interface IIndexSettingsApi
    {
        /// <summary>
        /// Returns all index settings.
        /// </summary>
        /// <returns>A collection of index settings.</returns>
        [Get("/api/IndexSettings")]
        Task<IReadOnlyCollection<IndexSettingsModel>> GetAllAsync();

        /// <summary>
        /// Returns index settings by index name.
        /// </summary>
        /// <param name="indexName">The name of index.</param>
        /// <returns>The model that describes index settings.</returns>
        [Get("/api/IndexSettings/{indexName}")]
        Task<IndexSettingsModel> GetByIndexAsync(string indexName);

        /// <summary>
        /// Adds new index settings.
        /// </summary>
        /// <param name="model">The model that describes index settings.</param>
        [Post("/api/IndexSettings")]
        Task AddAsync([Body] IndexSettingsModel model);

        /// <summary>
        /// Updates existing index settings.
        /// </summary>
        /// <param name="model">The model that describes index settings.</param>
        [Put("/api/IndexSettings")]
        Task UpdateAsync([Body] IndexSettingsModel model);

        /// <summary>
        /// Deletes index settings by index name.
        /// </summary>
        /// <param name="indexName">The name of index.</param>
        [Delete("/api/IndexSettings/{indexName}")]
        Task DeleteAsync(string indexName);
    }
}
