using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ITimersSettingsService
    {
        Task<TimersSettings> GetAsync();

        Task UpdateAsync(TimersSettings timersSettings);
    }
}
