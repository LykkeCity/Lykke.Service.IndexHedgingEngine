using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settings
{
    [UsedImplicitly]
    public class HedgeSettingsService : IHedgeSettingsService
    {
        private const string CacheKey = "key";

        private readonly IHedgeSettingsRepository _hedgeSettingsRepository;
        private readonly InMemoryCache<HedgeSettings> _cache;

        public HedgeSettingsService(IHedgeSettingsRepository hedgeSettingsRepository)
        {
            _hedgeSettingsRepository = hedgeSettingsRepository;
            _cache = new InMemoryCache<HedgeSettings>(settings => CacheKey, true);
        }

        public async Task<HedgeSettings> GetAsync()
        {
            HedgeSettings hedgeSettings = _cache.Get(CacheKey);

            if (hedgeSettings == null)
            {
                hedgeSettings = await _hedgeSettingsRepository.GetAsync();

                if (hedgeSettings == null)
                {
                    hedgeSettings = new HedgeSettings
                    {
                        MarketOrderMarkup = .02m,
                        ThresholdDown = 1000,
                        ThresholdUp = 5000,
                        ThresholdCritical = 10000
                    };
                }

                _cache.Initialize(new[] {hedgeSettings});
            }

            return hedgeSettings;
        }

        public async Task UpdateAsync(HedgeSettings hedgeSettings)
        {
            await _hedgeSettingsRepository.InsertOrReplaceAsync(hedgeSettings);

            _cache.Set(hedgeSettings);
        }
    }
}
