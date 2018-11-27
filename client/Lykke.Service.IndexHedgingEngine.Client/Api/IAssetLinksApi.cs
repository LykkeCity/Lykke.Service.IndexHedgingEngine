using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.AssetLinks;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with asset links.
    /// </summary>
    [PublicAPI]
    public interface IAssetLinksApi
    {
        /// <summary>
        /// Returns all asset links.
        /// </summary>
        /// <returns>A collection of asset links.</returns>
        [Get("/api/AssetLinks")]
        Task<IReadOnlyCollection<AssetLinkModel>> GetAllAsync();

        /// <summary>
        /// Returns missed asset links.
        /// </summary>
        /// <returns>A collection of asset identifiers.</returns>
        [Get("/api/AssetLinks/missed")]
        Task<IReadOnlyCollection<string>> GetMissedAsync();
        
        /// <summary>
        /// Adds new asset link.
        /// </summary>
        /// <param name="model">The model that describes asset link.</param>
        [Post("/api/AssetLinks")]
        Task AddAsync([Body] AssetLinkModel model);

        /// <summary>
        /// Updates existing asset link.
        /// </summary>
        /// <param name="model">The model that describes asset link.</param>
        [Put("/api/AssetLinks")]
        Task UpdateAsync([Body] AssetLinkModel model);

        /// <summary>
        /// Deletes asset link.
        /// </summary>
        /// <param name="assetId">The identifier of asset.</param>
        [Delete("/api/AssetLinks/{assetId}")]
        Task DeleteAsync(string assetId);
    }
}
