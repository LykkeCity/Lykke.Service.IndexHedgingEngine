using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.OrderBooks;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with order books.
    /// </summary>
    [PublicAPI]
    public interface IOrderBooksApi
    {
        /// <summary>
        /// Returns all order books that includes clients limit orders limited by levels. 
        /// </summary>
        /// <param name="limit">The number of limit orders per side.</param>
        /// <returns>A collection of order books.</returns>
        [Get("/api/OrderBooks/lykke")]
        Task<IReadOnlyCollection<OrderBookModel>> GetLykkeAsync(int limit);

        /// <summary>
        /// Returns all internal order books that contains market maker limit orders. 
        /// </summary>
        /// <returns>A collection of order books.</returns>
        [Get("/api/OrderBooks/internal")]
        Task<IReadOnlyCollection<OrderBookModel>> GetInternalAsync();
    }
}
