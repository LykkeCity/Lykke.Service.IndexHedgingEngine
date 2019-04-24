using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Trades
{
    public class VirtualTradeService : IVirtualTradeService
    {
        private readonly IVirtualTradeRepository _virtualTradeRepository;

        public VirtualTradeService(IVirtualTradeRepository virtualTradeRepository)
        {
            _virtualTradeRepository = virtualTradeRepository;
        }

        public Task<IReadOnlyCollection<VirtualTrade>> GetAsync(DateTime startDate, DateTime endDate,
            string assetPairId, int limit)
        {
            return _virtualTradeRepository.GetAsync(startDate, endDate, assetPairId, limit);
        }

        public Task AddAsync(VirtualTrade virtualTrade)
        {
            return _virtualTradeRepository.InsertAsync(virtualTrade);
        }
    }
}
