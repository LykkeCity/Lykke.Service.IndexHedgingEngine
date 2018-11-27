using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IMarketMakerService
    {
        Task UpdateOrderBookAsync(Index index);
    }
}
