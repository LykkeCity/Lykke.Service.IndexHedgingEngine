using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IHedgeService
    {
        Task ExecuteAsync();
    }
}
