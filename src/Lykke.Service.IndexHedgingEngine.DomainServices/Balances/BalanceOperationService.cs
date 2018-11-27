using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Balances
{
    [UsedImplicitly]
    public class BalanceOperationService : IBalanceOperationService
    {
        private readonly IBalanceOperationRepository _balanceOperationRepository;

        public BalanceOperationService(IBalanceOperationRepository balanceOperationRepository)
        {
            _balanceOperationRepository = balanceOperationRepository;
        }

        public Task<IReadOnlyCollection<BalanceOperation>> GetAsync(DateTime startDate, DateTime endDate, int limit)
        {
            return _balanceOperationRepository.GetAsync(startDate, endDate, limit);
        }

        public Task AddAsync(BalanceOperation balanceOperation)
        {
            return _balanceOperationRepository.InsertAsync(balanceOperation);
        }
    }
}
