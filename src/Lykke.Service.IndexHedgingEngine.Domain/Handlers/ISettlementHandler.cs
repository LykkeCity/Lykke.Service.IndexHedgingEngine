using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Handlers
{
    public interface ISettlementHandler
    {
        Task ExecuteAsync();
    }
}
