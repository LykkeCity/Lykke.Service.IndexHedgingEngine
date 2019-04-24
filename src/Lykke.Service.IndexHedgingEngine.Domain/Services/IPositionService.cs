using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IPositionService
    {
        Task<IReadOnlyCollection<Position>> GetAllAsync();

        Task<Position> GetByAssetIdAsync(string assetId, string exchange);

        Task UpdateAsync(string assetId, string exchange, TradeType tradeType, decimal volume, decimal oppositeVolume);

        Task CloseAsync(string assetId, string exchange, decimal volume, decimal price);

        Task DeleteAsync(string assetId, string exchange);
    }
}
