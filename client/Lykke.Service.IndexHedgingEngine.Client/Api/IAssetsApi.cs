using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Assets;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with assets.
    /// </summary>
    [PublicAPI]
    public interface IAssetsApi
    {
        /// <summary>
        /// Returns all assets.
        /// </summary>
        /// <returns>A collection of asset settings.</returns>
        [Get("/api/Assets")]
        Task<IReadOnlyCollection<AssetSettingsModel>> GetAllAsync();

        /// <summary>
        /// Adds new asset settings.
        /// </summary>
        /// <param name="model">The model that describes asset settings.</param>
        /// <param name="userId">The identifier of the user who added asset settings.</param>
        [Post("/api/Assets")]
        Task AddAsync([Body] AssetSettingsModel model, string userId);

        /// <summary>
        /// Updates existing asset settings.
        /// </summary>
        /// <param name="model">The model that describes asset settings.</param>
        /// <param name="userId">The identifier of the user who updated asset settings.</param>
        [Put("/api/Assets")]
        Task UpdateAsync([Body] AssetSettingsModel model, string userId);

        /// <summary>
        /// Deletes asset settings.
        /// </summary>
        /// <param name="asset">The name of the asset.</param>
        /// <param name="exchange">The name of exchange.</param>
        /// <param name="userId">The identifier of the user who deleted asset settings.</param>
        [Delete("/api/Assets")]
        Task DeleteAsync(string asset, string exchange, string userId);
    }
}
