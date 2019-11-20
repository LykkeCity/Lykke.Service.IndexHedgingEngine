using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IMarketMakerService
    {
        Task UpdateLimitOrdersAsync(string indexName);

        Task UpdateCrossPairLimitOrdersAsync(CrossAssetPairSettings crossAssetPairSettings);

        Task CancelLimitOrdersAsync(string indexName);

        Task CancelCrossPairLimitOrdersAsync(CrossAssetPairSettings crossAssetPairSettings);
    }
}
