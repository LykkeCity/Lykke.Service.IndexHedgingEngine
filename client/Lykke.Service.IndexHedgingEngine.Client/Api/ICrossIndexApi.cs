using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settings;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with cross indices.
    /// </summary>
    [PublicAPI]
    public interface ICrossIndexApi
    {
        /// <summary>
        /// Returns all cross indices.
        /// </summary>
        /// <returns>A collection of cross indices.</returns>
        [Get("/api/CrossIndices")]
        Task<IReadOnlyCollection<CrossIndexSettingsModel>> GetAllAsync();

        /// <summary>
        /// Adds new cross index settings.
        /// </summary>
        /// <param name="model">The model that describes cross index settings.</param>
        /// <param name="userId">The identifier of the user who added cross index settings.</param>
        [Post("/api/CrossIndices")]
        Task<Guid> AddAsync([Body] CrossIndexSettingsModel model, string userId);

        /// <summary>
        /// Updates existing cross index settings.
        /// </summary>
        /// <param name="model">The model that describes cross index settings.</param>
        /// <param name="userId">The identifier of the user who updated cross index settings.</param>
        [Put("/api/CrossIndices")]
        Task UpdateAsync([Body] CrossIndexSettingsModel model, string userId);

        /// <summary>
        /// Deletes cross index settings.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="userId">The identifier of the user who deleted cross index settings.</param>
        [Delete("/api/CrossIndices")]
        Task DeleteAsync(Guid id, string userId);
    }
}
