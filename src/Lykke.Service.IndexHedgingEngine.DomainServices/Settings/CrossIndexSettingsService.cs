using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settings
{
    public class CrossIndexSettingsService : ICrossIndexSettingsService
    {
        private readonly ICrossIndexSettingsRepository _crossIndexSettingsRepository;
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IInstrumentService _instrumentService;
        private readonly ILog _log;
        
        private readonly InMemoryCache<CrossIndexSettings> _crossIndicesCache;

        public CrossIndexSettingsService(
            ICrossIndexSettingsRepository crossIndexSettingsRepository,
            IIndexSettingsService indexSettingsService,
            IInstrumentService instrumentService,
            ILogFactory logFactory)
        {
            _crossIndexSettingsRepository = crossIndexSettingsRepository;
            _indexSettingsService = indexSettingsService;
            _instrumentService = instrumentService;
            _log = logFactory.CreateLog(this);
            
            _crossIndicesCache = new InMemoryCache<CrossIndexSettings>(GetKey, false);
        }

        public async Task<IReadOnlyCollection<CrossIndexSettings>> GetAllAsync()
        {
            IReadOnlyCollection<CrossIndexSettings> crossAssetPairsSettings = _crossIndicesCache.GetAll();

            if (crossAssetPairsSettings == null)
            {
                crossAssetPairsSettings = await _crossIndexSettingsRepository.GetAllAsync();

                _crossIndicesCache.Initialize(crossAssetPairsSettings);
            }

            return crossAssetPairsSettings;
        }
        
        public async Task<CrossIndexSettings> GetAsync(Guid id)
        {
            IReadOnlyCollection<CrossIndexSettings> assetPairsSettings = await GetAllAsync();

            return assetPairsSettings.SingleOrDefault(o => o.Id == id);
        }

        public async Task<IReadOnlyList<CrossIndexSettings>> FindByIndexAssetPairAsync(string indexAssetPairId)
        {
            IReadOnlyCollection<CrossIndexSettings> assetPairsSettings = await GetAllAsync();

            return assetPairsSettings.Where(o => o.IndexAssetPairId == indexAssetPairId).ToList();
        }

        public async Task<Guid> AddAsync(CrossIndexSettings entity, string userId)
        {
            if (entity.Id == null)
                entity.Id = Guid.NewGuid();

            await ValidateAsync(entity);

            if (await GetAsync(entity.Id.Value) != null)
                throw new EntityAlreadyExistsException();

            await _crossIndexSettingsRepository.InsertAsync(entity);

            _crossIndicesCache.Set(entity);

            _log.InfoWithDetails("Cross asset pair settings added.", new { entity, userId });

            return entity.Id.Value;
        }

        public async Task UpdateAsync(CrossIndexSettings entity, string userId)
        {
            if (entity?.Id == null)
                throw new ArgumentNullException(nameof(entity.Id));

            CrossIndexSettings existed = await GetAsync(entity.Id.Value);

            if (existed == null)
                throw new EntityNotFoundException();

            await ValidateAsync(entity);

            await _crossIndexSettingsRepository.UpdateAsync(entity);

            _crossIndicesCache.Set(entity);

            _log.InfoWithDetails("Cross asset pair settings updated.", new { entity, userId });
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            CrossIndexSettings existed = await GetAsync(id);

            if (existed == null)
                throw new EntityNotFoundException();

            await _crossIndexSettingsRepository.DeleteAsync(id);

            _crossIndicesCache.Remove(id.ToString());

            _log.InfoWithDetails("Cross asset settings deleted.", new { existed, userId });
        }

        private async Task ValidateAsync(CrossIndexSettings entity)
        {
            var indexAssetPair = (await _indexSettingsService.GetAllAsync())
                .SingleOrDefault(x => x.AssetPairId == entity.IndexAssetPairId);

            if (indexAssetPair == null)
                throw new InvalidOperationException("Index asset pair not found.");

            var crossAssetPair = (await _instrumentService.GetAssetPairsAsync())
                .SingleOrDefault(x => x.Exchange == entity.Exchange && x.AssetPairId == entity.AssetPairId);

            if (crossAssetPair == null)
                throw new InvalidOperationException("Asset pair not found.");
        }

        private static string GetKey(CrossIndexSettings entity)
            => GetKey(entity.Id);

        private static string GetKey(Guid? id)
            => $"{id}";
    }
}
