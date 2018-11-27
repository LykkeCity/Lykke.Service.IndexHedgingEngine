using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.MarketMaker;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods to work with market maker state.
    /// </summary>
    [PublicAPI]
    public interface IMarketMakerApi
    {
        /// <summary>
        /// Returns current market maker state.
        /// </summary>
        /// <returns>The model that describes market maker state.</returns>
        [Get("/api/marketmaker/state")]
        Task<MarketMakerStateModel> GetStateAsync();

        /// <summary>
        /// Updates market maker state.
        /// </summary>
        /// <param name="model">The model that describes market maker state changes.</param>
        [Post("/api/marketmaker/state")]
        Task SetStateAsync([Body] MarketMakerStateUpdateModel model);
    }
}
