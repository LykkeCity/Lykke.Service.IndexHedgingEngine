using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IExternalTradeRepository
    {
        Task<IReadOnlyCollection<ExternalTrade>> GetAsync(DateTime startDate, DateTime endDate, string exchange,
            string assetPairId, int limit);

        Task InsertAsync(ExternalTrade externalTrade);
    }
}
