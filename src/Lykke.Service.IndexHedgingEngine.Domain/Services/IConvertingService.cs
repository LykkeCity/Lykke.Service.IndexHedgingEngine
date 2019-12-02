using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IConvertingService
    {
        Task<decimal?> ConvertToUsdAsync(string assetIdFrom, decimal value);
    }
}
