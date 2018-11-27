using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IMarketMakerStateRepository
    {
        Task<MarketMakerState> GetAsync();

        Task InsertOrReplaceAsync(MarketMakerState marketMakerState);
    }
}
