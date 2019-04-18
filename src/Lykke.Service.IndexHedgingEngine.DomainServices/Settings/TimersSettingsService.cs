using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settings
{
    [UsedImplicitly]
    public class TimersSettingsService : ITimersSettingsService
    {
        private const string CacheKey = "key";

        private readonly ITimersSettingsRepository _timersSettingsRepository;
        private readonly InMemoryCache<TimersSettings> _cache;

        public TimersSettingsService(ITimersSettingsRepository timersSettingsRepository)
        {
            _timersSettingsRepository = timersSettingsRepository;
            _cache = new InMemoryCache<TimersSettings>(settings => CacheKey, true);
        }

        public async Task<TimersSettings> GetAsync()
        {
            TimersSettings timersSettings = _cache.Get(CacheKey);

            if (timersSettings == null)
            {
                timersSettings = await _timersSettingsRepository.GetAsync();

                if (timersSettings == null)
                {
                    timersSettings = new TimersSettings
                    {
                        LykkeBalances = TimeSpan.FromSeconds(10),
                        ExternalBalances = TimeSpan.FromSeconds(10),
                        Settlements = TimeSpan.FromMinutes(5)
                    };
                }

                _cache.Initialize(new[] {timersSettings});
            }

            return timersSettings;
        }

        public async Task UpdateAsync(TimersSettings timersSettings)
        {
            await _timersSettingsRepository.InsertOrReplaceAsync(timersSettings);

            _cache.Set(timersSettings);
        }
    }
}
