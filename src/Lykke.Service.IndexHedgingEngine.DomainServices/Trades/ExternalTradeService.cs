using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Trades
{
    public class ExternalTradeService : IExternalTradeService
    {
        private readonly IExternalTradeRepository _externalTradeRepository;

        public ExternalTradeService(IExternalTradeRepository externalTradeRepository)
        {
            _externalTradeRepository = externalTradeRepository;
        }

        public Task<IReadOnlyCollection<ExternalTrade>> GetAsync(DateTime startDate, DateTime endDate, string exchange,
            string assetPairId, int limit)
        {
            return _externalTradeRepository.GetAsync(startDate, endDate, exchange, assetPairId, limit);
        }

        public Task RegisterAsync(ExternalTrade externalTrade)
        {
            return _externalTradeRepository.InsertAsync(externalTrade);
        }
    }
}
