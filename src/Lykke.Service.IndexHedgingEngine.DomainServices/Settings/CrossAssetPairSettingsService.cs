﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;
using MoreLinq;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settings
{
    [UsedImplicitly]
    public class CrossAssetPairSettingsService : ICrossAssetPairSettingsService
    {
        private const string LykkeExchangeName = "lykke";

        private readonly ICrossAssetPairSettingsRepository _crossAssetPairSettingsRepository;
        private readonly IInstrumentService _instrumentService;
        private readonly ILog _log;
        private readonly InMemoryCache<CrossAssetPairSettings> _cache;

        public CrossAssetPairSettingsService(ICrossAssetPairSettingsRepository crossAssetPairSettingsRepository,
            IInstrumentService instrumentService,
            ILogFactory logFactory)
        {
            _crossAssetPairSettingsRepository = crossAssetPairSettingsRepository;
            _instrumentService = instrumentService;
            _log = logFactory.CreateLog(this);
            _cache = new InMemoryCache<CrossAssetPairSettings>(GetKey, false);
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

        public async Task<CrossAssetPairSettings> FindByAssetPairIdAsync(string assetPairId)
        {
            var allCrossPairs = await GetAllAsync();

            var result = allCrossPairs.FirstOrDefault(x => x.AssetPairId == assetPairId);

            return result;
        }

        public async Task<CrossAssetPairSettings> FindByIdAsync(Guid id)
        {
            var allCrossPairs = await GetAllAsync();

            var result = allCrossPairs.FirstOrDefault(x => x.Id == id);

            return result;
        }

        public async Task<IReadOnlyCollection<CrossAssetPairSettings>> FindEnabledByIndexAsync(string indexName, string shortIndexName)
        {
            var allCrossPairs = await GetAllAsync();

            List<CrossAssetPairSettings> crossPairsToUpdate = allCrossPairs
                .Where(x => x.Mode == CrossAssetPairSettingsMode.Enabled).ToList();

            crossPairsToUpdate = crossPairsToUpdate.Where(x => x.BaseAsset == indexName || x.QuoteAsset == indexName).ToList();

            if (shortIndexName != null)
            {
                var shortIndexCrossPairs = allCrossPairs.Where(x => x.BaseAsset == shortIndexName 
                                                                 || x.QuoteAsset == shortIndexName);

                crossPairsToUpdate.AddRange(shortIndexCrossPairs);
            }

            var result = crossPairsToUpdate.DistinctBy(x => x.Id).ToList().AsReadOnly();

            return result;
        }

        public async Task AddCrossAssetPairAsync(CrossAssetPairSettings crossAssetPairSettings, string userId)
        {
            CrossAssetPairSettings existingAssetPairSettings =
                await FindByAssetPairIdAsync(crossAssetPairSettings.AssetPairId);

            if (crossAssetPairSettings.Id != Guid.Empty)
                throw new InvalidOperationException("Id must be empty");

            if (existingAssetPairSettings != null)
                throw new EntityAlreadyExistsException();

            crossAssetPairSettings.Id = Guid.NewGuid();

            await ValidateCrossAssetPairSettingsAsync(crossAssetPairSettings);

            await _crossAssetPairSettingsRepository.InsertAsync(crossAssetPairSettings);

            _cache.Set(crossAssetPairSettings);

            _log.InfoWithDetails("Cross asset pair settings added", new { crossAssetPairSettings, userId });
        }

        public async Task UpdateCrossAssetPairAsync(CrossAssetPairSettings crossAssetPairSettings, string userId)
        {
           CrossAssetPairSettings existingAssetPairSettings = await FindByIdAsync(crossAssetPairSettings.Id);

            if (existingAssetPairSettings == null)
                throw new EntityNotFoundException();

            existingAssetPairSettings.BuySpread = crossAssetPairSettings.BuySpread;
            existingAssetPairSettings.BuyVolume = crossAssetPairSettings.BuyVolume;
            existingAssetPairSettings.SellSpread = crossAssetPairSettings.SellSpread;
            existingAssetPairSettings.SellVolume = crossAssetPairSettings.SellVolume;

            await ValidateCrossAssetPairSettingsAsync(crossAssetPairSettings);

            await _crossAssetPairSettingsRepository.UpdateAsync(crossAssetPairSettings);

            _cache.Set(crossAssetPairSettings);

            _log.InfoWithDetails("Cross asset pair settings updated", new { crossAssetPairSettings, userId });
        }

        public async Task UpdateModeAsync(Guid id, CrossAssetPairSettingsMode mode, string userId)
        {
            CrossAssetPairSettings crossAssetPairSettings = await FindByIdAsync(id);

            if (crossAssetPairSettings == null)
                throw new EntityNotFoundException();

            crossAssetPairSettings.Mode = mode;

            await ValidateCrossAssetPairSettingsAsync(crossAssetPairSettings);

            await _crossAssetPairSettingsRepository.UpdateAsync(crossAssetPairSettings);

            _cache.Set(crossAssetPairSettings);

            _log.InfoWithDetails("Cross asset pair settings mode changed", new { crossAssetPairSettings, mode, userId });
        }

        public async Task DeleteCrossAssetPairAsync(Guid id, string userId)
        {
            CrossAssetPairSettings existingAssetPairSettings = await FindByIdAsync(id);

            if (existingAssetPairSettings == null)
                throw new EntityNotFoundException();

            if (existingAssetPairSettings.Mode != CrossAssetPairSettingsMode.Disabled)
                throw new InvalidOperationException("Cross asset pair has to be stopped before deleting.");

            await _crossAssetPairSettingsRepository.DeleteAsync(id);

            _cache.Remove(GetKey(existingAssetPairSettings));

            _log.InfoWithDetails("Cross asset pair settings deleted", new { existingAssetPairSettings, userId });
        }

        private async Task ValidateCrossAssetPairSettingsAsync(CrossAssetPairSettings crossAssetPairSettings)
        {
            AssetSettings baseAssetSettings = await _instrumentService.GetAssetAsync(crossAssetPairSettings.BaseAsset, LykkeExchangeName);

            if (baseAssetSettings == null)
                throw new InvalidOperationException("Base asset not found");

            AssetSettings quoteAssetSettings = await _instrumentService.GetAssetAsync(crossAssetPairSettings.QuoteAsset, LykkeExchangeName);

            if (quoteAssetSettings == null)
                throw new InvalidOperationException("Quote asset not found");
        }

        private static string GetKey(CrossAssetPairSettings crossAssetPairSettings)
            => GetKey(crossAssetPairSettings.AssetPairId);

        private static string GetKey(string assetPairId)
            => $"{assetPairId}";
    }
}
