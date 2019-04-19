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
        
        public async Task<CrossAssetPairSettings> GetAsync(string assetPairId, string crossAssetPairId)
        {
            IReadOnlyCollection<CrossAssetPairSettings> assetPairsSettings = await GetAllAsync();

            return assetPairsSettings.SingleOrDefault(o => o.AssetPairId == assetPairId && o.CrossAssetPairId == crossAssetPairId);
        }

        public async Task AddAsync(CrossAssetPairSettings crossAssetPairSettings, string userId)
        {
            CrossAssetPairSettings existed =
                await GetAsync(crossAssetPairSettings.AssetPairId, crossAssetPairSettings.CrossAssetPairId);

            if (existed != null)
                throw new EntityAlreadyExistsException();

            await ValidateAsync(crossAssetPairSettings);

            await _crossAssetPairSettingsRepository.InsertAsync(crossAssetPairSettings);

            _crossAssetPairsCache.Set(crossAssetPairSettings);

            _log.InfoWithDetails("Cross asset pair settings added", new { assetPairSettings = crossAssetPairSettings, userId });
        }

        public async Task UpdateAsync(CrossAssetPairSettings crossAssetPairSettings, string userId)
        {
            CrossAssetPairSettings existed =
                await GetAsync(crossAssetPairSettings.AssetPairId, crossAssetPairSettings.CrossAssetPairId);

            if (existed == null)
                throw new EntityNotFoundException();

            await ValidateAsync(crossAssetPairSettings);

            await _crossAssetPairSettingsRepository.UpdateAsync(crossAssetPairSettings);

            _crossAssetPairsCache.Set(crossAssetPairSettings);

            _log.InfoWithDetails("Cross asset pair settings updated", new { assetPairSettings = crossAssetPairSettings, userId });
        }

        public async Task DeleteAsync(string assetPairId, string crossAssetPairId, string userId)
        {
            CrossAssetPairSettings existed = await GetAsync(assetPairId, crossAssetPairId);

            if (existed == null)
                throw new EntityNotFoundException();

            await _crossAssetPairSettingsRepository.DeleteAsync(assetPairId, crossAssetPairId);

            _crossAssetPairsCache.Remove(GetKey(assetPairId, crossAssetPairId));

            _log.InfoWithDetails("Cross asset settings deleted", new { existed, userId });
        }

        private async Task ValidateAsync(CrossAssetPairSettings crossAssetPairSettings)
        {
            var indexAssetPair = (await _indexSettingsService.GetAllAsync())
                .SingleOrDefault(x => x.AssetPairId == crossAssetPairSettings.AssetPairId);

            if (indexAssetPair == null)
                throw new InvalidOperationException("Asset pair not found");

            var crossAssetPair = (await _instrumentService.GetAssetPairsAsync())
                .SingleOrDefault(x => x.AssetPairId == crossAssetPairSettings.CrossAssetPairId);

            if (crossAssetPair == null)
                throw new InvalidOperationException("Cross asset pair not found");
        }

        private static string GetKey(CrossAssetPairSettings crossAssetPairSettings)
            => GetKey(crossAssetPairSettings.AssetPairId, crossAssetPairSettings.CrossAssetPairId);

        private static string GetKey(string assetPairId, string crossAssetPairId)
            => $"{assetPairId}_{crossAssetPairId}";
    }
}
