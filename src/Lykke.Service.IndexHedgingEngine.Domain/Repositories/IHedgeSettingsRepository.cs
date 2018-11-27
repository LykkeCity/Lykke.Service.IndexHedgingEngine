using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IHedgeSettingsRepository
    {
        Task<HedgeSettings> GetAsync();

        Task InsertOrReplaceAsync(HedgeSettings hedgeSettings);
    }
}
