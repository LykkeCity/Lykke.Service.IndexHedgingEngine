using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
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

        public async Task<IReadOnlyList<CrossIndexSettings>> FindByOriginalIndexAssetPairAsync(string indexAssetPairId)
        {
            IReadOnlyCollection<CrossIndexSettings> assetPairsSettings = await GetAllAsync();

            return assetPairsSettings.Where(o => o.OriginalAssetPairId == indexAssetPairId).ToList();
        }

        public async Task<Guid> AddAsync(CrossIndexSettings entity, string userId)
        {
            if (entity.Id == null)
                entity.Id = Guid.NewGuid();

            await ValidateAsync(entity);

            await SetPropertiesAsync(entity);

            if (await GetAsync(entity.Id.Value) != null)
                throw new EntityAlreadyExistsException();

            await _crossIndexSettingsRepository.InsertAsync(entity);

            _crossIndicesCache.Set(entity);

            _log.InfoWithDetails("Cross index settings added.", new { entity, userId });

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

            await SetPropertiesAsync(entity);

            await _crossIndexSettingsRepository.UpdateAsync(entity);

            _crossIndicesCache.Set(entity);

            _log.InfoWithDetails("Cross index settings updated.", new { entity, userId });
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            CrossIndexSettings existed = await GetAsync(id);

            if (existed == null)
                throw new EntityNotFoundException();

            await _crossIndexSettingsRepository.DeleteAsync(id);

            _crossIndicesCache.Remove(id.ToString());

            _log.InfoWithDetails("Cross index settings deleted.", new { existed, userId });
        }

        private async Task ValidateAsync(CrossIndexSettings entity)
        {
            IndexSettings indexSettings = (await _indexSettingsService.GetAllAsync())
                .SingleOrDefault(x => x.AssetPairId == entity.OriginalAssetPairId);
            
            if (indexSettings == null)
                throw new InvalidOperationException("Original index settings not found.");

            IReadOnlyCollection<AssetPairSettings> allAssetPairs = await _instrumentService.GetAssetPairsAsync();

            AssetPairSettings indexAssetPair = allAssetPairs
                .SingleOrDefault(x => x.AssetPairId == entity.OriginalAssetPairId);

            if (indexAssetPair == null)
                throw new InvalidOperationException("Original index asset pair not found.");

            AssetPairSettings crossAssetPair = allAssetPairs
                .SingleOrDefault(x => x.Exchange == entity.Exchange && x.AssetPairId == entity.CrossAssetPairId);

            if (crossAssetPair == null)
                throw new InvalidOperationException("Cross asset pair not found.");

            bool isAssetToAssetValid = entity.IsInverted
                ? indexAssetPair.QuoteAsset == crossAssetPair.QuoteAsset
                : indexAssetPair.QuoteAsset == crossAssetPair.BaseAsset;

            if (!isAssetToAssetValid)
                throw new InvalidOperationException("Original index asset pair can not be matched with cross asset pair.");

            IReadOnlyList<string> existedQuoteAssets = (await FindByOriginalIndexAssetPairAsync(entity.OriginalAssetPairId))
                .Select(x => x.QuoteAssetId)
                .ToList();

            string currentQuoteAsset = (await GetCrossIndexAssetPairSettings(indexSettings, entity)).QuoteAsset;

            if (existedQuoteAssets.Contains(currentQuoteAsset))
                throw new InvalidOperationException("Cross index with the same quote asset already existed.");

            string quoteAsset = entity.IsInverted
                ? crossAssetPair.BaseAsset
                : crossAssetPair.QuoteAsset;

            if (currentQuoteAsset != quoteAsset)
                throw new InvalidOperationException("Quote assets are not equal.");

            AssetPairSettings assetPair = allAssetPairs.SingleOrDefault(x =>
                x.Exchange == ExchangeNames.Lykke &&
                x.BaseAsset == indexAssetPair.BaseAsset &&
                x.QuoteAsset == quoteAsset);

            if (assetPair == null)
                throw new InvalidOperationException($"There is no {indexAssetPair.BaseAsset}/{quoteAsset} on {nameof(ExchangeNames.Lykke)}.");
        }

        private async Task SetPropertiesAsync(CrossIndexSettings entity)
        {
            IndexSettings indexSettings = (await _indexSettingsService.GetAllAsync())
                .Single(x => x.AssetPairId == entity.OriginalAssetPairId);

            if (indexSettings == null)
                throw new InvalidOperationException("Original index settings not found.");

            AssetPairSettings assetPairSettings = await GetCrossIndexAssetPairSettings(indexSettings, entity);

            entity.AssetPairId = assetPairSettings.AssetPairId;

            entity.AssetId = assetPairSettings.BaseAsset;

            entity.QuoteAssetId = assetPairSettings.QuoteAsset;
        }

        private async Task<AssetPairSettings> GetCrossIndexAssetPairSettings(IndexSettings indexSettings,
            CrossIndexSettings crossIndexSettings)
        {
            AssetPairSettings indexAssetPairSettings =
                await _instrumentService.GetAssetPairAsync(indexSettings.AssetPairId, ExchangeNames.Lykke);

            if (indexAssetPairSettings == null)
                throw new InvalidOperationException("Original index asset pair settings not found");

            AssetSettings indexBaseAssetSettings =
                await _instrumentService.GetAssetAsync(indexAssetPairSettings.BaseAsset, ExchangeNames.Lykke);

            if (indexBaseAssetSettings == null)
                throw new InvalidOperationException("Original index base asset settings not found");

            AssetPairSettings crossAssetPairSettings =
                await _instrumentService.GetAssetPairAsync(crossIndexSettings.CrossAssetPairId, crossIndexSettings.Exchange);

            if (crossAssetPairSettings == null)
                throw new InvalidOperationException("Cross asset pair not found");

            AssetSettings crossAssetPairBaseAssetSettings =
                await _instrumentService.GetAssetAsync(crossAssetPairSettings.BaseAsset, ExchangeNames.Lykke);

            if (crossAssetPairBaseAssetSettings == null)
                throw new InvalidOperationException("Cross base asset settings not found");

            AssetSettings crossAssetPairQuoteAssetSettings =
                await _instrumentService.GetAssetAsync(crossAssetPairSettings.QuoteAsset, ExchangeNames.Lykke);

            if (crossAssetPairQuoteAssetSettings == null)
                throw new InvalidOperationException("Cross quote asset settings not found");

            AssetSettings crossIndexBaseAssetSettings = indexBaseAssetSettings;

            AssetSettings crossIndexQuoteAssetSettings =
                crossIndexSettings.IsInverted ? crossAssetPairBaseAssetSettings : crossAssetPairQuoteAssetSettings;

            AssetPairSettings crossIndexAssetPairSettings = await _instrumentService.GetAssetPairAsync(
                crossIndexBaseAssetSettings.AssetId, crossIndexQuoteAssetSettings.AssetId, ExchangeNames.Lykke);

            if (crossIndexAssetPairSettings == null)
                throw new InvalidOperationException("Cross index asset pair settings not found");

            return crossIndexAssetPairSettings;
        }

        private static string GetKey(CrossIndexSettings entity)
            => GetKey(entity.Id);

        private static string GetKey(Guid? id)
            => $"{id}";
    }
}
