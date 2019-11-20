using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using MoreLinq;

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

        public async Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAllAsync()
        {
            IReadOnlyCollection<CrossAssetPairSettings> crossAssetPairSettings = _cache.GetAll();

            if (crossAssetPairSettings == null)
            {
                crossAssetPairSettings = await _crossAssetPairSettingsRepository.GetAllAsync();

                _cache.Initialize(crossAssetPairSettings);
            }

            return crossAssetPairSettings;
        }

        public async Task<IReadOnlyCollection<CrossAssetPairSettings>> FindCrossAssetPairsByIndexAsync(string indexName, string shortIndexName)
        {
            var allCrossPairs = await GetAllAsync();

            List<CrossAssetPairSettings> crossPairsToUpdate =
                allCrossPairs.Where(x => x.BaseAsset == indexName || x.QuoteAsset == indexName).ToList();

            if (shortIndexName != null)
            {
                var shortIndexCrossPairs = allCrossPairs.Where(x => x.BaseAsset == shortIndexName 
                                                                 || x.QuoteAsset == shortIndexName);

                crossPairsToUpdate.AddRange(shortIndexCrossPairs);
            }

            var result = crossPairsToUpdate.DistinctBy(x => x.Id).ToList().AsReadOnly();

            return result;
        }
    }
}
