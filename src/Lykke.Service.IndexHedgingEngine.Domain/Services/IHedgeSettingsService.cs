using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IHedgeSettingsService
    {
        Task<HedgeSettings> GetAsync();

        Task UpdateAsync(HedgeSettings hedgeSettings);
    }
}
