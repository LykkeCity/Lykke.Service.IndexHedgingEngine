using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Handlers
{
    public interface IMarketMakerStateHandler
    {
        Task HandleMarketMakerStateAsync(MarketMakerStatus marketMakerStatus, string comment, string userId);
    }
}
