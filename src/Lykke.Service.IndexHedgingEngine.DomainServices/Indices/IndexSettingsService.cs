using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Indices
{
    [UsedImplicitly]
    public class IndexSettingsService : IIndexSettingsService
    {
        private readonly IIndexSettingsRepository _indexSettingsRepository;
        private readonly IInstrumentService _instrumentService;
        private readonly ILog _log;
        private readonly InMemoryCache<IndexSettings> _cache;

        public IndexSettingsService(
            IIndexSettingsRepository indexSettingsRepository,
            IInstrumentService instrumentService,
            IIndexPriceService indexPriceService,
            ILogFactory logFactory)
        {
            _indexSettingsRepository = indexSettingsRepository;
            _instrumentService = instrumentService;

            _cache = new InMemoryCache<IndexSettings>(GetKey, false);
            _log = logFactory.CreateLog(this);
        }

        public async Task<IReadOnlyCollection<IndexSettings>> GetAllAsync()
        {
            IReadOnlyCollection<IndexSettings> indexSettings = _cache.GetAll();

            if (indexSettings == null)
            {
                indexSettings = await _indexSettingsRepository.GetAllAsync();

                _cache.Initialize(indexSettings);
            }

            return indexSettings;
        }

        public async Task<IndexSettings> GetByIndexAsync(string indexName)
        {
            IReadOnlyCollection<IndexSettings> indexSettings = await GetAllAsync();

            return indexSettings.SingleOrDefault(o => o.Name == indexName);
        }

        public async Task AddAsync(IndexSettings indexSettings)
        {
            IndexSettings currentIndexSettings = await GetByIndexAsync(indexSettings.Name);

            if (currentIndexSettings != null)
                throw new EntityAlreadyExistsException();

            await ValidateAsync(indexSettings);

            await _indexSettingsRepository.InsertAsync(indexSettings);

            _cache.Set(indexSettings);

            _log.InfoWithDetails("Index settings added", indexSettings);
        }

        public async Task UpdateAsync(IndexSettings indexSettings)
        {
            IndexSettings currentIndexSettings = await GetByIndexAsync(indexSettings.Name);

            if (currentIndexSettings == null)
                throw new EntityNotFoundException();

            await ValidateAsync(indexSettings);

            currentIndexSettings.Update(indexSettings);

            await _indexSettingsRepository.UpdateAsync(currentIndexSettings);

            _cache.Set(currentIndexSettings);

            _log.InfoWithDetails("Index settings updated", currentIndexSettings);
        }

        public async Task DeleteAsync(string indexName)
        {
            IndexSettings currentIndexSettings = await GetByIndexAsync(indexName);

            if (currentIndexSettings == null)
                throw new EntityNotFoundException();

            await _indexSettingsRepository.DeleteAsync(indexName);

            _cache.Remove(GetKey(indexName));

            _log.InfoWithDetails("Index settings removed", currentIndexSettings);
        }

        private async Task ValidateAsync(IndexSettings indexSettings)
        {
            // TODO: Use internal asset id
            AssetSettings assetSettings = (await _instrumentService.GetAssetsAsync())
                .Single(o => o.Exchange == ExchangeNames.Lykke && o.AssetId == indexSettings.AssetId);

            if (assetSettings == null)
                throw new InvalidOperationException("Asset settings not found");

            AssetPairSettings assetPairSettings =
                await _instrumentService.GetAssetPairAsync(indexSettings.AssetPairId, ExchangeNames.Lykke);

            if (assetPairSettings == null)
                throw new InvalidOperationException("Asset pair settings not found");

            if (indexSettings.SellVolume / indexSettings.SellLimitOrdersCount < assetPairSettings.MinVolume)
                throw new InvalidOperationException("Sell limit order volume is less than min volume");

            if (indexSettings.BuyVolume / indexSettings.BuyLimitOrdersCount < assetPairSettings.MinVolume)
                throw new InvalidOperationException("Buy limit order volume is less than min volume");
        }

        private static string GetKey(IndexSettings indexSettings)
            => GetKey(indexSettings.Name);

        private static string GetKey(string indexName)
            => indexName.ToUpper();
    }
}
