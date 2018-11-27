using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.AssetHedgeSettings;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with asset hedge settings.
    /// </summary>
    [PublicAPI]
    public interface IAssetHedgeSettingsApi
    {
        /// <summary>
        /// Returns all asset hedge settings.
        /// </summary>
        /// <returns>A collection of asset hedge settings.</returns>
        [Get("/api/AssetHedgeSettings")]
        Task<IReadOnlyCollection<AssetHedgeSettingsModel>> GetAllAsync();

        /// <summary>
        /// Returns asset hedge settings by asset id.
        /// </summary>
        /// <param name="assetId">The identifier of asset.</param>
        /// <returns>The model that describes asset hedge settings.</returns>
        [Get("/api/AssetHedgeSettings/{assetId}")]
        Task<AssetHedgeSettingsModel> GetByIndexAsync(string assetId);

        /// <summary>
        /// Adds new asset hedge settings.
        /// </summary>
        /// <param name="model">The model that describes asset hedge settings.</param>
        [Post("/api/AssetHedgeSettings")]
        Task AddAsync([Body] AssetHedgeSettingsEditModel model);

        /// <summary>
        /// Updates existing asset hedge settings.
        /// </summary>
        /// <param name="model">The model that describes asset hedge settings.</param>
        [Put("/api/AssetHedgeSettings")]
        Task UpdateAsync([Body] AssetHedgeSettingsEditModel model);

        /// <summary>
        /// Deletes asset hedge settings by asset id.
        /// </summary>
        /// <param name="assetId">The identifier of asset.</param>
        [Delete("/api/AssetHedgeSettings/{assetId}")]
        Task DeleteAsync(string assetId);
    }
}
