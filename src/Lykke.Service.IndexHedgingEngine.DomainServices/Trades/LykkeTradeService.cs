using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Trades
{
    public class LykkeTradeService : ILykkeTradeService
    {
        private readonly ILykkeTradeRepository _lykkeTradeRepository;

        public LykkeTradeService(ILykkeTradeRepository lykkeTradeRepository)
        {
            _lykkeTradeRepository = lykkeTradeRepository;
        }

        public Task<IReadOnlyCollection<InternalTrade>> GetAsync(DateTime startDate, DateTime endDate,
            string assetPairId, string oppositeWalletId, int limit)
        {
            return _lykkeTradeRepository.GetAsync(startDate, endDate, assetPairId, oppositeWalletId, limit);
        }

        public Task<InternalTrade> GetByIdAsync(string internalTradeId)
        {
            return _lykkeTradeRepository.GetByIdAsync(internalTradeId);
        }

        public async Task<bool> ExistAsync(string internalTradeId)
        {
            InternalTrade internalTrade = await _lykkeTradeRepository.GetByIdAsync(internalTradeId);

            return internalTrade != null;
        }

        public Task RegisterAsync(InternalTrade internalTrade)
        {
            return _lykkeTradeRepository.InsertAsync(internalTrade);
        }
    }
}
