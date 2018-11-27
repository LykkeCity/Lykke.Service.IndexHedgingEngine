using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Handlers
{
    public interface IInternalHedgeTradeHandler
    {
        Task HandleInternalTradesAsync(IReadOnlyCollection<InternalTrade> internalTrades);
    }
}
