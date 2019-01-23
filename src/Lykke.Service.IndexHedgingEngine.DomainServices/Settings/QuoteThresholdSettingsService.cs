using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settings
{
    [UsedImplicitly]
    public class QuoteThresholdSettingsService : IQuoteThresholdSettingsService
    {
        private const string CacheKey = "key";

        private readonly IQuoteThresholdSettingsRepository _quoteThresholdSettingsRepository;
        private readonly InMemoryCache<QuoteThresholdSettings> _cache;

        public QuoteThresholdSettingsService(IQuoteThresholdSettingsRepository quoteThresholdSettingsRepository)
        {
            _quoteThresholdSettingsRepository = quoteThresholdSettingsRepository;
            _cache = new InMemoryCache<QuoteThresholdSettings>(settings => CacheKey, true);
        }

        public async Task<QuoteThresholdSettings> GetAsync()
        {
            QuoteThresholdSettings quoteThresholdSettings = _cache.Get(CacheKey);

            if (quoteThresholdSettings == null)
            {
                quoteThresholdSettings = await _quoteThresholdSettingsRepository.GetAsync();

                if (quoteThresholdSettings == null)
                {
                    quoteThresholdSettings = new QuoteThresholdSettings
                    {
                        Enabled = true,
                        Value = .2m
                    };
                }

                _cache.Initialize(new[] {quoteThresholdSettings});
            }

            return quoteThresholdSettings;
        }

        public async Task UpdateAsync(QuoteThresholdSettings quoteThresholdSettings)
        {
            await _quoteThresholdSettingsRepository.InsertOrReplaceAsync(quoteThresholdSettings);

            _cache.Set(quoteThresholdSettings);
        }
    }
}
