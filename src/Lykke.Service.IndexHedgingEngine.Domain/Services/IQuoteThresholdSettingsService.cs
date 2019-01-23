using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IQuoteThresholdSettingsService
    {
        Task<QuoteThresholdSettings> GetAsync();

        Task UpdateAsync(QuoteThresholdSettings quoteThresholdSettings);
    }
}
