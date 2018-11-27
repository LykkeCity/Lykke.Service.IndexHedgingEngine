using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ITimersSettingsService
    {
        Task<TimersSettings> GetAsync();

        Task UpdateAsync(TimersSettings timersSettings);
    }
}
