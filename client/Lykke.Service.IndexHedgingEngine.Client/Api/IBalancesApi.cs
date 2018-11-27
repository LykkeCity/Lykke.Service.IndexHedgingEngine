using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Balances;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with balances.
    /// </summary>
    [PublicAPI]
    public interface IBalancesApi
    {
        /// <summary>
        /// Returns a collection of balances from Lykke exchange.
        /// </summary>
        /// <returns>A collection of balances.</returns>
        [Get("/api/Balances/lykke")]
        Task<IReadOnlyList<BalanceModel>> GetLykkeAsync();
    }
}
