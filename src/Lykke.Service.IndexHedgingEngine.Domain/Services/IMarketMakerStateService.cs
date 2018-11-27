using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IMarketMakerStateService
    {
        Task<MarketMakerState> GetAsync();

        Task UpdateAsync(MarketMakerStatus marketMakerStatus, string comment, string userId);
    }
}
