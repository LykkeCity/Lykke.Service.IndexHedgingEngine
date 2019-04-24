using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ILykkeTradeService
    {
        Task<IReadOnlyCollection<InternalTrade>> GetAsync(DateTime startDate, DateTime endDate, string assetPairId,
            string oppositeWalletId, int limit);

        Task<InternalTrade> GetByIdAsync(string internalTradeId);

        Task<bool> ExistAsync(string internalTradeId);

        Task RegisterAsync(InternalTrade internalTrade);
    }
}
