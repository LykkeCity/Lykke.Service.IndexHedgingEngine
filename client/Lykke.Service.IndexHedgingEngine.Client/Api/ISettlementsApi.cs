using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settlements;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with settlements.
    /// </summary>
    [PublicAPI]
    public interface ISettlementsApi
    {
        /// <summary>
        /// Returns a collection of settlements.
        /// </summary>
        /// <param name="clientId">The identifier fo the client.</param>
        /// <returns>A collection of settlements.</returns>
        [Get("/api/Settlements")]
        Task<IReadOnlyCollection<SettlementModel>> GetAsync(string clientId = null);

        /// <summary>
        /// Returns a settlement by identifier.
        /// </summary>
        /// <param name="settlementId">The identifier fo the settlement.</param>
        /// <returns>The model that describes a settlement.</returns>
        [Get("/api/Settlements/{settlementId}")]
        Task<SettlementModel> GetByIdAsync(string settlementId);

        /// <summary>
        /// Creates new settlement.
        /// </summary>
        /// <param name="model">The model that describes a settlement.</param>
        /// <param name="userId">The identifier of the user who requested the settlement.</param>
        [Post("/api/Settlements")]
        Task CreateAsync([Body] SettlementRequestModel model, string userId);

        /// <summary>
        /// Approve the settlement.
        /// </summary>
        /// <param name="settlementId">The identifier fo the settlement.</param>
        /// <param name="userId">The identifier of the user who approved the settlement.</param>
        [Post("/api/Settlements/{settlementId}/approve")]
        Task ApproveAsync(string settlementId, string userId);

        /// <summary>
        /// Reject the settlement.
        /// </summary>
        /// <param name="settlementId">The identifier fo the settlement.</param>
        /// <param name="userId">The identifier of the user who rejected the settlement.</param>
        [Post("/api/Settlements/{settlementId}/reject")]
        Task RejectAsync(string settlementId, string userId);

        /// <summary>
        /// Recalculate the settlement.
        /// </summary>
        /// <param name="settlementId">The identifier fo the settlement.</param>
        /// <param name="userId">The identifier of the user who recalculated the settlement.</param>
        [Post("/api/Settlements/{settlementId}/recalculate")]
        Task RecalculateAsync(string settlementId, string userId);

        /// <summary>
        /// Validate the settlement.
        /// </summary>
        /// <param name="settlementId">The identifier fo the settlement.</param>
        /// <param name="userId">The identifier of the user who validated the settlement.</param>
        [Post("/api/Settlements/{settlementId}/validate")]
        Task ValidateAsync(string settlementId, string userId);
        
        /// <summary>
        /// Updates the asset settlement.
        /// </summary>
        /// <param name="model">The model that describes an asset settlement.</param>
        /// <param name="settlementId">The identifier fo the settlement.</param>
        /// <param name="userId">The identifier of the user who recalculated the settlement.</param>
        [Put("/api/Settlements/{settlementId}/assets")]
        Task UpdateAssetAsync([Body] AssetSettlementEditModel model, string settlementId, string userId);
        
        /// <summary>
        /// Updates the asset settlement.
        /// </summary>
        /// <param name="settlementId">The identifier fo the settlement.</param>
        /// <param name="assetId">The identifier fo the asset.</param>
        /// <param name="userId">The identifier of the user who recalculated the settlement.</param>
        [Post("/api/Settlements/{settlementId}/assets/{assetId}/retry")]
        Task RetryAssetAsync(string settlementId, string assetId, string userId);
    }
}
