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

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Instruments
{
    public class InstrumentService : IInstrumentService
    {
        private readonly IAssetSettingsRepository _assetSettingsRepository;
        private readonly IAssetPairSettingsRepository _assetPairSettingsRepository;
        private readonly IAssetHedgeSettingsService _assetHedgeSettingsService;
        private readonly ILog _log;

        private readonly InMemoryCache<AssetSettings> _assetsCache;
        private readonly InMemoryCache<AssetPairSettings> _assetPairsCache;

        private readonly HashSet<string> _assetPairs = new HashSet<string>();

        public InstrumentService(
            IAssetSettingsRepository assetSettingsRepository,
            IAssetPairSettingsRepository assetPairSettingsRepository,
            IAssetHedgeSettingsService assetHedgeSettingsService,
            ILogFactory logFactory)
        {
            _assetSettingsRepository = assetSettingsRepository;
            _assetPairSettingsRepository = assetPairSettingsRepository;
            _assetHedgeSettingsService = assetHedgeSettingsService;
            _log = logFactory.CreateLog(this);

            _assetsCache = new InMemoryCache<AssetSettings>(GetKey, false);
            _assetPairsCache = new InMemoryCache<AssetPairSettings>(GetKey, false);
        }

        public async Task<IReadOnlyCollection<AssetSettings>> GetAssetsAsync()
        {
            IReadOnlyCollection<AssetSettings> assetsSettings = _assetsCache.GetAll();

            if (assetsSettings == null)
            {
                assetsSettings = await _assetSettingsRepository.GetAllAsync();

                _assetsCache.Initialize(assetsSettings);
            }

            return assetsSettings;
        }

        public async Task<IReadOnlyCollection<AssetPairSettings>> GetAssetPairsAsync()
        {
            IReadOnlyCollection<AssetPairSettings> assetPairsSettings = _assetPairsCache.GetAll();

            if (assetPairsSettings == null)
            {
                assetPairsSettings = await _assetPairSettingsRepository.GetAllAsync();

                _assetPairsCache.Initialize(assetPairsSettings);

                InitializeAssetPairs();
            }

            return assetPairsSettings;
        }

        public async Task<AssetSettings> GetAssetAsync(string asset, string exchange)
        {
            IReadOnlyCollection<AssetSettings> assetsSettings = await GetAssetsAsync();

            return assetsSettings.SingleOrDefault(o => o.Asset == asset && o.Exchange == exchange);
        }

        public async Task<AssetPairSettings> GetAssetPairAsync(string assetPair, string exchange)
        {
            IReadOnlyCollection<AssetPairSettings> assetPairsSettings = await GetAssetPairsAsync();

            return assetPairsSettings.SingleOrDefault(o => o.AssetPair == assetPair && o.Exchange == exchange);
        }

        public async Task AddAssetAsync(AssetSettings assetSettings, string userId)
        {
            AssetSettings existingAssetSettings = await GetAssetAsync(assetSettings.Asset, assetSettings.Exchange);

            if (existingAssetSettings != null)
                throw new EntityAlreadyExistsException();

            await _assetSettingsRepository.InsertAsync(assetSettings);

            _assetsCache.Set(assetSettings);

            _log.InfoWithDetails("Asset settings added", new {assetSettings, userId});
        }

        public async Task AddAssetPairAsync(AssetPairSettings assetPairSettings, string userId)
        {
            AssetPairSettings existingAssetPairSettings =
                await GetAssetPairAsync(assetPairSettings.AssetPair, assetPairSettings.Exchange);

            if (existingAssetPairSettings != null)
                throw new EntityAlreadyExistsException();

            await ValidateAssetPairSettingsAsync(assetPairSettings);

            await _assetPairSettingsRepository.InsertAsync(assetPairSettings);

            _assetPairsCache.Set(assetPairSettings);

            InitializeAssetPairs();

            _log.InfoWithDetails("Asset pair settings added", new {assetPairSettings, userId});
        }

        public async Task UpdateAssetAsync(AssetSettings assetSettings, string userId)
        {
            AssetSettings existingAssetSettings = await GetAssetAsync(assetSettings.Asset, assetSettings.Exchange);

            if (existingAssetSettings == null)
                throw new EntityNotFoundException();

            await _assetSettingsRepository.UpdateAsync(assetSettings);

            _assetsCache.Set(assetSettings);

            _log.InfoWithDetails("Asset settings updated", new {assetSettings, userId});
        }

        public async Task UpdateAssetPairAsync(AssetPairSettings assetPairSettings, string userId)
        {
            AssetPairSettings existingAssetPairSettings =
                await GetAssetPairAsync(assetPairSettings.AssetPair, assetPairSettings.Exchange);

            if (existingAssetPairSettings == null)
                throw new EntityNotFoundException();

            await ValidateAssetPairSettingsAsync(assetPairSettings);

            await _assetPairSettingsRepository.UpdateAsync(assetPairSettings);

            _assetPairsCache.Set(assetPairSettings);

            _log.InfoWithDetails("Asset pair settings updated", new {assetPairSettings, userId});
        }

        public async Task DeleteAssetAsync(string asset, string exchange, string userId)
        {
            AssetSettings existingAssetSettings = await GetAssetAsync(asset, exchange);

            if (existingAssetSettings == null)
                throw new EntityNotFoundException();

            IReadOnlyCollection<AssetPairSettings> assetPairsSettings = await GetAssetPairsAsync();

            if (assetPairsSettings.Any(o => o.Exchange == exchange && (o.BaseAsset == asset || o.QuoteAsset == asset)))
                throw new InvalidOperationException("Asset is used by asset pair.");

            await _assetSettingsRepository.DeleteAsync(asset, exchange);

            _assetsCache.Remove(GetKey(asset, exchange));

            _log.InfoWithDetails("Asset settings deleted", new {existingAssetSettings, userId});
        }

        public async Task DeleteAssetPairAsync(string assetPair, string exchange, string userId)
        {
            AssetPairSettings existingAssetPairSettings = await GetAssetPairAsync(assetPair, exchange);

            if (existingAssetPairSettings == null)
                throw new EntityNotFoundException();

            IReadOnlyCollection<AssetHedgeSettings> assetHedgeSettings = await _assetHedgeSettingsService.GetAllAsync();

            if (assetHedgeSettings.Any(o => o.Exchange == exchange && o.AssetPairId == assetPair))
                throw new InvalidOperationException("Asset pair is used by asset hedge settings.");

            await _assetPairSettingsRepository.DeleteAsync(assetPair, exchange);

            _assetPairsCache.Remove(GetKey(assetPair, exchange));

            InitializeAssetPairs();

            _log.InfoWithDetails("Asset settings deleted", new {existingAssetPairSettings, userId});
        }

        public async Task<bool> IsAssetPairExistAsync(string assetPair)
        {
            if (!_assetPairsCache.Initialized)
                await GetAssetPairsAsync();

            return _assetPairs.Contains(assetPair);
        }

        private void InitializeAssetPairs()
        {
            _assetPairs.Clear();

            foreach (AssetPairSettings assetPairSettings in _assetPairsCache.GetAll())
                _assetPairs.Add(assetPairSettings.AssetPair);
        }

        private async Task ValidateAssetPairSettingsAsync(AssetPairSettings assetPairSettings)
        {
            AssetSettings baseAssetSettings = await GetAssetAsync(assetPairSettings.BaseAsset,
                assetPairSettings.Exchange);

            if (baseAssetSettings == null)
                throw new InvalidOperationException("Base asset not found");

            AssetSettings quoteAssetSettings = await GetAssetAsync(assetPairSettings.QuoteAsset,
                assetPairSettings.Exchange);

            if (quoteAssetSettings == null)
                throw new InvalidOperationException("Quote asset not found");
        }

        private static string GetKey(AssetSettings assetSettings)
            => GetKey(assetSettings.Asset, assetSettings.Exchange);

        private static string GetKey(AssetPairSettings assetPairSettings)
            => GetKey(assetPairSettings.AssetPair, assetPairSettings.Exchange);

        private static string GetKey(string instrument, string exchange)
            => $"{instrument}_{exchange}";
    }
}
