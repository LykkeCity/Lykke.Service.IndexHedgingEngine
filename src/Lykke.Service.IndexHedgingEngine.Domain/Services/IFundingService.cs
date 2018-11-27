using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IFundingService
    {
        Task<Funding> GetAsync();

        Task UpdateAsync(BalanceOperationType balanceOperationType, decimal amount, string comment, string userId);
    }
}
