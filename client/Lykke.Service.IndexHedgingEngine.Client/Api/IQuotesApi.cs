using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with quotes.
    /// </summary>
    [PublicAPI]
    public interface IQuotesApi
    {
        /// <summary>
        /// Returns all quotes by exchange. 
        /// </summary>
        /// <param name="exchange">The name of the exchange.</param>
        /// <returns>A collection of quotes.</returns>
        [Get("/api/Quotes")]
        Task<IReadOnlyCollection<QuoteModel>> GetAsync(string exchange);
    }
}
