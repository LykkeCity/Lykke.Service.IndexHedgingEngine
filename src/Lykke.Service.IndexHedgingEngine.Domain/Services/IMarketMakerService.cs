using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IMarketMakerService
    {
        Task UpdateLimitOrdersAsync(string indexName);

        Task UpdateCrossPairLimitOrders(CrossAssetPairSettings crossAssetPairSettings);

        Task CancelLimitOrdersAsync(string indexName);

        Task CancelCrossPairLimitOrders(CrossAssetPairSettings crossAssetPairSettings);
    }
}
