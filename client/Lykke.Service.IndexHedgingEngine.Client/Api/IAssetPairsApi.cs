using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.AssetPairs;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with asset pairs.
    /// </summary>
    [PublicAPI]
    public interface IAssetPairsApi
    {
        /// <summary>
        /// Returns all asset pairs.
        /// </summary>
        /// <returns>A collection of asset pairs.</returns>
        [Get("/api/AssetPairs")]
        Task<IReadOnlyCollection<AssetPairSettingsModel>> GetAllAsync();

        /// <summary>
        /// Adds new asset pair settings.
        /// </summary>
        /// <param name="model">The model that describes asset pair settings.</param>
        /// <param name="userId">The identifier of the user who added asset pair settings.</param>
        [Post("/api/AssetPairs")]
        Task AddAsync([Body] AssetPairSettingsModel model, string userId);

        /// <summary>
        /// Updates existing asset pair settings.
        /// </summary>
        /// <param name="model">The model that describes asset pair settings.</param>
        /// <param name="userId">The identifier of the user who updated asset pair settings.</param>
        [Put("/api/AssetPairs")]
        Task UpdateAsync([Body] AssetPairSettingsModel model, string userId);

        /// <summary>
        /// Deletes asset pair settings.
        /// </summary>
        /// <param name="assetPair">The name of the asset pair.</param>
        /// <param name="exchange">The name of exchange.</param>
        /// <param name="userId">The identifier of the user who deleted asset pair settings.</param>
        [Delete("/api/AssetPairs")]
        Task DeleteAsync(string assetPair, string exchange, string userId);
    }
}
