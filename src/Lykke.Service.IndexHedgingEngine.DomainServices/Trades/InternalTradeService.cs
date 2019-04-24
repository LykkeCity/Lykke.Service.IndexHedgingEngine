using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Trades
{
    [UsedImplicitly]
    public class InternalTradeService : IInternalTradeService
    {
        private readonly IInternalTradeRepository _internalTradeRepository;

        public InternalTradeService(IInternalTradeRepository internalTradeRepository)
        {
            _internalTradeRepository = internalTradeRepository;
        }

        public Task<IReadOnlyCollection<InternalTrade>> GetAsync(DateTime startDate, DateTime endDate,
            string assetPairId, string oppositeWalletId, int limit)
        {
            return _internalTradeRepository.GetAsync(startDate, endDate, assetPairId, oppositeWalletId, limit);
        }

        public Task<InternalTrade> GetByIdAsync(string internalTradeId)
        {
            return _internalTradeRepository.GetByIdAsync(internalTradeId);
        }

        public Task RegisterAsync(InternalTrade internalTrade)
        {
            return _internalTradeRepository.InsertAsync(internalTrade);
        }
    }
}
