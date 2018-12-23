using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IHedgeService
    {
        Task UpdateLimitOrdersAsync();

        Task CreateLimitOrderAsync(string assetId, string exchange, LimitOrderType limitOrderType, decimal price,
            decimal volume, string userId);

        Task CancelLimitOrderAsync(string assetId, string exchange, string userId);

        Task ClosePositionAsync(string assetId, string exchange, string userId);
    }
}
