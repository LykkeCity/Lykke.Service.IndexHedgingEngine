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
    public class CrossAssetPairSettingsService : ICrossAssetPairSettingsService
    {
        private readonly ICrossAssetPairSettingsRepository _crossAssetPairSettingsRepository;
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IInstrumentService _instrumentService;
        private readonly ILog _log;
        
        private readonly InMemoryCache<CrossAssetPairSettings> _crossAssetPairsCache;

        public CrossAssetPairSettingsService(
            ICrossAssetPairSettingsRepository crossAssetPairSettingsRepository,
            IIndexSettingsService indexSettingsService,
            IInstrumentService instrumentService,
            ILogFactory logFactory)
        {
            _crossAssetPairSettingsRepository = crossAssetPairSettingsRepository;
            _indexSettingsService = indexSettingsService;
            _instrumentService = instrumentService;
            _log = logFactory.CreateLog(this);
            
            _crossAssetPairsCache = new InMemoryCache<CrossAssetPairSettings>(GetKey, false);
        }

        public async Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAllAsync()
        {
            IReadOnlyCollection<CrossAssetPairSettings> crossAssetPairsSettings = _crossAssetPairsCache.GetAll();

            if (crossAssetPairsSettings == null)
            {
                crossAssetPairsSettings = await _crossAssetPairSettingsRepository.GetAllAsync();

                _crossAssetPairsCache.Initialize(crossAssetPairsSettings);
            }

            return crossAssetPairsSettings;
        }
        
        public async Task<CrossAssetPairSettings> GetAsync(Guid id)
        {
            IReadOnlyCollection<CrossAssetPairSettings> assetPairsSettings = await GetAllAsync();

            return assetPairsSettings.SingleOrDefault(o => o.Id == id);
        }

        public async Task<Guid> AddAsync(CrossAssetPairSettings entity, string userId)
        {
            if (entity.Id == null)
                entity.Id = Guid.NewGuid();

            await ValidateAsync(entity);

            if (await GetAsync(entity.Id.Value) != null)
                throw new EntityAlreadyExistsException();

            await _crossAssetPairSettingsRepository.InsertAsync(entity);

            _crossAssetPairsCache.Set(entity);

            _log.InfoWithDetails("Cross asset pair settings added.", new { entity, userId });

            return entity.Id.Value;
        }

        public async Task UpdateAsync(CrossAssetPairSettings entity, string userId)
        {
            if (entity?.Id == null)
                throw new ArgumentNullException(nameof(entity.Id));

            CrossAssetPairSettings existed = await GetAsync(entity.Id.Value);

            if (existed == null)
                throw new EntityNotFoundException();

            await ValidateAsync(entity);

            await _crossAssetPairSettingsRepository.UpdateAsync(entity);

            _crossAssetPairsCache.Set(entity);

            _log.InfoWithDetails("Cross asset pair settings updated.", new { entity, userId });
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            CrossAssetPairSettings existed = await GetAsync(id);

            if (existed == null)
                throw new EntityNotFoundException();

            await _crossAssetPairSettingsRepository.DeleteAsync(id);

            _crossAssetPairsCache.Remove(id.ToString());

            _log.InfoWithDetails("Cross asset settings deleted.", new { existed, userId });
        }

        private async Task ValidateAsync(CrossAssetPairSettings entity)
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

        private static string GetKey(CrossAssetPairSettings entity)
            => GetKey(entity.Id);

        private static string GetKey(Guid? id)
            => $"{id}";
    }
}
