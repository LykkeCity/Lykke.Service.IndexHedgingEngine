using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IExternalOrderRepository
    {
        Task<ExternalOrder> GetAsync(string exchange, string asset);

        Task InsertAsync(ExternalOrder externalOrder);

        Task DeleteAsync(string exchange, string asset);
    }
}
