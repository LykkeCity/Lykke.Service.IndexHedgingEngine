using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;

namespace Lykke.Service.IndexHedgingEngine.Domain.Handlers
{
    public interface IInternalTradeHandler
    {
        Task HandleInternalTradesAsync(IReadOnlyCollection<InternalTrade> internalTrades);
    }
}
