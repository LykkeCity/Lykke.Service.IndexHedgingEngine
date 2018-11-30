using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IBalanceOperationService
    {
        Task<IReadOnlyCollection<BalanceOperation>> GetAsync(DateTime startDate, DateTime endDate, int limit,
            string assetId, BalanceOperationType balanceOperationType);

        Task AddAsync(BalanceOperation balanceOperation);
    }
}
