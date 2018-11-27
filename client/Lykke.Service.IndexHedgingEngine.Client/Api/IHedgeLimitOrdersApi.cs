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
    }
}
