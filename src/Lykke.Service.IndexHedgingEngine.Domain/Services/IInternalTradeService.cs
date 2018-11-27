using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IInternalTradeService
    {
        Task<IReadOnlyCollection<InternalTrade>> GetAsync(DateTime startDate, DateTime endDate, string assetPairId,
            string oppositeWalletId, int limit);

        Task<InternalTrade> GetByIdAsync(string internalTradeId);

        Task RegisterAsync(InternalTrade internalTrade);
    }
}
