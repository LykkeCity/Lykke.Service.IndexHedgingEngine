using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IFundingRepository
    {
        Task<Funding> GetAsync();

        Task InsertOrReplaceAsync(Funding funding);
    }
}
