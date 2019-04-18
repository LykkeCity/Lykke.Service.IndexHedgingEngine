using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface IQuoteThresholdSettingsRepository
    {
        Task<QuoteThresholdSettings> GetAsync();

        Task InsertOrReplaceAsync(QuoteThresholdSettings quoteThresholdSettings);
    }
}
