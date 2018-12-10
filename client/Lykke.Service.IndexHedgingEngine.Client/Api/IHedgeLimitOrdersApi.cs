using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.HedgeLimitOrders;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with hedge limit orders.
    /// </summary>
    [PublicAPI]
    public interface IHedgeLimitOrdersApi
    {
        /// <summary>
        /// Returns all hedge limit orders. 
        /// </summary>
        /// <returns>A collection of hedge limit orders.</returns>
        [Get("/api/HedgeLimitOrders")]
        Task<IReadOnlyCollection<HedgeLimitOrderModel>> GetAllAsync();

        /// <summary>
        /// Creates hedge limit order.
        /// </summary>
        /// <param name="model">The model that describes hedge limit order creation details.</param>
        /// <param name="userId">The identifier of the user who created hedge limit order.</param>
        [Post("/api/HedgeLimitOrders/create")]
        Task CreateAsync([Body] HedgeLimitOrderCreateModel model, string userId);
        
        /// <summary>
        /// Creates hedge limit order.
        /// </summary>
        /// <param name="model">The model that describes hedge limit order cancellation details.</param>
        /// <param name="userId">The identifier of the user who cancel hedge limit order.</param>
        [Post("/api/HedgeLimitOrders/cancel")]
        Task CancelAsync([Body] HedgeLimitOrderCancelModel model, string userId);
    }
}
