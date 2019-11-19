using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settings
{
    [UsedImplicitly]
    public class CrossAssetPairSettingsService : ICrossAssetPairSettingsService
    {
        private const string CacheKey = "key";

        private readonly ICrossAssetPairSettingsRepository _crossAssetPairSettingsRepository;
        private readonly InMemoryCache<CrossAssetPairSettings> _cache;

        public CrossAssetPairSettingsService(ICrossAssetPairSettingsRepository crossAssetPairSettingsRepository)
        {
            _crossAssetPairSettingsRepository = crossAssetPairSettingsRepository;
            _cache = new InMemoryCache<CrossAssetPairSettings>(settings => CacheKey, true);
        }

        public async Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAll()
        {
            IReadOnlyCollection<CrossAssetPairSettings> crossAssetPairSettings = _cache.GetAll();

            if (crossAssetPairSettings == null)
            {
                crossAssetPairSettings = await _crossAssetPairSettingsRepository.GetAllAsync();

                _cache.Initialize(crossAssetPairSettings);
            }

            return crossAssetPairSettings;
        }
    }
}
