using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ITokenService
    {
        Task<IReadOnlyCollection<Token>> GetAllAsync();

        Task<Token> GetAsync(string assetId);

        Task UpdateAmountAsync(string assetId, BalanceOperationType balanceOperationType, decimal amount,
            string comment, string userId);

        Task UpdateVolumeAsync(string assetId, InternalTrade internalTrade);

        Task CloseAsync(string assetId, decimal volume, decimal price);
    }
}
