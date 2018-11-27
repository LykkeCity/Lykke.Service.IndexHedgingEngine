using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Infrastructure
{
    public interface IExchangeAdapter
    {
        string Name { get; }

        Task CancelLimitOrderAsync(string assetId);
        
        Task ExecuteLimitOrderAsync(HedgeLimitOrder hedgeLimitOrder);
    }
}
