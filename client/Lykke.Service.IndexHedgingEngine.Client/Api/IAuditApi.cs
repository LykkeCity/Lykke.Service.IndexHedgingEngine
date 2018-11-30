using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models;
using Lykke.Service.IndexHedgingEngine.Client.Models.Audit;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with audit report.
    /// </summary>
    [PublicAPI]
    public interface IAuditApi
    {
        /// <summary>
        /// Returns a collection of balance operations for the period.
        /// </summary>
        /// <param name="startDate">The start date of period.</param>
        /// <param name="endDate">The end date of period.</param>
        /// <param name="limit">The maximum number of operations.</param>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="type">The type of a balance operation.</param>
        /// <returns>A collection of balance operations.</returns>
        [Get("/api/Audit/balances")]
        Task<IReadOnlyCollection<BalanceOperationModel>> GetBalanceOperationsAsync(DateTime startDate, DateTime endDate,
            int limit, string assetId, BalanceOperationType type);
    }
}
