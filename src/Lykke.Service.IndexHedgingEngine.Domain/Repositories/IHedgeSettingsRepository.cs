using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IHedgeSettingsRepository
    {
        Task<HedgeSettings> GetAsync();

        Task InsertOrReplaceAsync(HedgeSettings hedgeSettings);
    }
}
