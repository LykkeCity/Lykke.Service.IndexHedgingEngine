using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.PrimaryMarket;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Api to interact with Primary Market wallet
    /// </summary>
    [PublicAPI]
    public interface IPrimaryMarketApi
    {
        /// <summary>
        /// Method to request general info about Primary Market.
        /// </summary>
        /// <returns>General info about Primary Market.</returns>
        [Get("/api/PrimaryMarket/info")]
        Task<PrimaryMarketInfoModel> GetInfoAsync();
        
        /// <summary>
        /// Method to request Primary Market wallet balances.
        /// </summary>
        /// <returns>List of non-zero balances of Primary Market wallet.</returns>
        [Get("/api/PrimaryMarket/balances")]
        Task<IReadOnlyList<PrimaryMarketBalanceModel>> GetBalancesAsync();
        
        /// <summary>
        /// Method to make changes to Primary Market wallet balances.
        /// </summary>
        /// <param name="assetId">Asset Id.</param>
        /// <param name="amount">Amount to add or subtract from current balance (value can be negative).</param>
        /// <param name="userId">Backoffice user.</param>
        /// <param name="comment">A comment of why the change is performed.</param>
        [Put("/api/PrimaryMarket/update")]
        Task ChangeBalanceAsync(string assetId, decimal amount, string userId, string comment);
        
        /// <summary>
        /// Method to request history of balance changes of Primary Market wallet.
        /// </summary>
        /// <returns>A list of balance changes of Primary Market wallet.</returns>
        [Get("/api/PrimaryMarket/history")]
        Task<IReadOnlyList<PrimaryMarketHistoryItemModel>> GetBalanceChangeHistoryAsync();
    }
}
