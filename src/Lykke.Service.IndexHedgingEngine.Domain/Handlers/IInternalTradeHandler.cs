using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Handlers
{
    public interface IInternalTradeHandler
    {
        Task HandleInternalTradesAsync(IReadOnlyCollection<InternalTrade> internalTrades);
    }
}
