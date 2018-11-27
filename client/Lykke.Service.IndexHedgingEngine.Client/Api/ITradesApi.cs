using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Trades;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with trades.
    /// </summary>
    [PublicAPI]
    public interface ITradesApi
    {
        /// <summary>
        /// Returns a collection of trades from Lykke exchange by period.
        /// </summary>
        /// <param name="startDate">The start date of period.</param>
        /// <param name="endDate">The end date of period.</param>
        /// <param name="assetPairId">The identifier of asset pair.</param>
        /// <param name="oppositeWalletId">The identifier of opposite wallet.</param>
        /// <param name="limit">The maximum number of trades.</param>
        /// <returns>A collection of internal trades.</returns>
        [Get("/api/trades/internal")]
        Task<IReadOnlyCollection<InternalTradeModel>> GetInternalTradesAsync(DateTime startDate, DateTime endDate,
            string assetPairId, string oppositeWalletId, int limit);

        /// <summary>
        /// Returns a collection of trades from virtual exchange by period.
        /// </summary>
        /// <param name="startDate">The start date of period.</param>
        /// <param name="endDate">The end date of period.</param>
        /// <param name="assetPairId">The identifier of asset pair.</param>
        /// <param name="limit">The maximum number of trades.</param>
        /// <returns>A collection of virtual trades.</returns>
        [Get("/api/trades/virtual")]
        Task<IReadOnlyCollection<VirtualTradeModel>> GetVirtualTradesAsync(DateTime startDate, DateTime endDate,
            string assetPairId, int limit);
        
        /// <summary>
        /// Returns a collection of trades from Lykke exchange by period.
        /// </summary>
        /// <param name="startDate">The start date of period.</param>
        /// <param name="endDate">The end date of period.</param>
        /// <param name="assetPairId">The identifier of asset pair.</param>
        /// <param name="oppositeWalletId">The identifier of opposite wallet.</param>
        /// <param name="limit">The maximum number of trades.</param>
        /// <returns>A collection of Lykke trades.</returns>
        [Get("/api/trades/lykke")]
        Task<IReadOnlyCollection<InternalTradeModel>> GetLykkeTradesAsync(DateTime startDate, DateTime endDate,
            string assetPairId, string oppositeWalletId, int limit);

        /// <summary>
        /// Returns an internal trade by id.
        /// </summary>
        /// <param name="tradeId">The trade id.</param>
        /// <returns>An internal trade.</returns>
        [Get("/api/trades/internal/{tradeId}")]
        Task<InternalTradeModel> GetInternalTradeByIdAsync(string tradeId);
    }
}
