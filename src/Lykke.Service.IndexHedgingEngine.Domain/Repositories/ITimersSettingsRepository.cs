using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface ITimersSettingsRepository
    {
        Task<TimersSettings> GetAsync();

        Task InsertOrReplaceAsync(TimersSettings timersSettings);
    }
}
