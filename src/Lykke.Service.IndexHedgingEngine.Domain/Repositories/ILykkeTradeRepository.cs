using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface ILykkeTradeRepository
    {
        Task<IReadOnlyCollection<InternalTrade>> GetAsync(DateTime startDate, DateTime endDate, string assetPairId,
            string oppositeWalletId, int limit);

        Task<InternalTrade> GetByIdAsync(string internalTradeId);

        Task InsertAsync(InternalTrade internalTrade);
    }
}
