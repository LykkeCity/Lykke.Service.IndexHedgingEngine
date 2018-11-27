using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Positions;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with positions.
    /// </summary>
    [PublicAPI]
    public interface IPositionsApi
    {
        /// <summary>
        /// Returns all positions. 
        /// </summary>
        /// <returns>A collection of positions.</returns>
        [Get("/api/Positions")]
        Task<IReadOnlyCollection<PositionModel>> GetAllAsync();

        /// <summary>
        /// Closes position by current quote.
        /// </summary>
        /// <param name="model">The model that describes a close operation.</param>
        [Post("/api/Positions")]
        Task CloseAsync([Body] ClosePositionOperationModel model);
    }
}
