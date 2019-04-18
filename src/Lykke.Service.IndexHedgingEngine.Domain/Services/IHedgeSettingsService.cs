using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IHedgeSettingsService
    {
        Task<HedgeSettings> GetAsync();

        Task UpdateAsync(HedgeSettings hedgeSettings);
    }
}
