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
        
        public async Task<CrossAssetPairSettings> GetAsync(string indexAssetPairId, string exchange, string assetPairId)
        {
            IReadOnlyCollection<CrossAssetPairSettings> assetPairsSettings = await GetAllAsync();

            return assetPairsSettings.SingleOrDefault(o =>
                o.IndexAssetPairId == indexAssetPairId &&
                o.Exchange == exchange &&
                o.AssetPairId == assetPairId);
        }

        public async Task AddAsync(CrossAssetPairSettings entity, string userId)
        {
            CrossAssetPairSettings existed =
                await GetAsync(entity.IndexAssetPairId, entity.Exchange, entity.AssetPairId);

            if (existed != null)
                throw new EntityAlreadyExistsException();

            await ValidateAsync(entity);

            await _crossAssetPairSettingsRepository.InsertAsync(entity);

            _crossAssetPairsCache.Set(entity);

            _log.InfoWithDetails("Cross asset pair settings added", new { assetPairSettings = entity, userId });
        }

        public async Task UpdateAsync(CrossAssetPairSettings entity, string userId)
        {
            CrossAssetPairSettings existed =
                await GetAsync(entity.IndexAssetPairId, entity.Exchange, entity.AssetPairId);

            if (existed == null)
                throw new EntityNotFoundException();

            await ValidateAsync(entity);

            await _crossAssetPairSettingsRepository.UpdateAsync(entity);

            _crossAssetPairsCache.Set(entity);

            _log.InfoWithDetails("Cross asset pair settings updated", new { assetPairSettings = entity, userId });
        }

        public async Task DeleteAsync(string indexAssetPairId, string exchange, string assetPairId, string userId)
        {
            CrossAssetPairSettings existed = await GetAsync(indexAssetPairId, exchange, assetPairId);

            if (existed == null)
                throw new EntityNotFoundException();

            await _crossAssetPairSettingsRepository.DeleteAsync(indexAssetPairId, exchange, assetPairId);

            _crossAssetPairsCache.Remove(GetKey(indexAssetPairId, exchange, assetPairId));

            _log.InfoWithDetails("Cross asset settings deleted", new { existed, userId });
        }

        private async Task ValidateAsync(CrossAssetPairSettings entity)
        {
            var indexAssetPair = (await _indexSettingsService.GetAllAsync())
                .SingleOrDefault(x => x.AssetPairId == entity.IndexAssetPairId);

            if (indexAssetPair == null)
                throw new InvalidOperationException("Index asset pair not found");

            var crossAssetPair = (await _instrumentService.GetAssetPairsAsync())
                .SingleOrDefault(x => x.Exchange == entity.Exchange && x.AssetPairId == entity.AssetPairId);

            if (crossAssetPair == null)
                throw new InvalidOperationException("Asset pair not found");
        }

        private static string GetKey(CrossAssetPairSettings entity)
            => GetKey(entity.IndexAssetPairId, entity.Exchange, entity.AssetPairId);

        private static string GetKey(string indexAssetPairId, string exchange, string assetPairId)
            => $"{indexAssetPairId}_{exchange}_{assetPairId}";
    }
}
