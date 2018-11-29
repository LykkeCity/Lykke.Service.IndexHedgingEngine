using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IBalanceOperationRepository
    {
        Task<IReadOnlyCollection<BalanceOperation>> GetAsync(DateTime startDate, DateTime endDate, int limit,
            string assetId, BalanceOperationType balanceOperationType);

        Task InsertAsync(BalanceOperation balanceOperation);
    }
}
