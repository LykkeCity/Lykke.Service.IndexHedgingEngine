using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Handlers
{
    public interface IIndexHandler
    {
        Task HandleIndexAsync(Index index);
    }
}
