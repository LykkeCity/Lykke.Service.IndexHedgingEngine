using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Hedging
{
    public class AssetHedgeSettingsService : IAssetHedgeSettingsService
    {
        private readonly IAssetHedgeSettingsRepository _assetHedgeSettingsRepository;
        private readonly ILog _log;
        private readonly InMemoryCache<AssetHedgeSettings> _cache;

        public AssetHedgeSettingsService(
            IAssetHedgeSettingsRepository assetHedgeSettingsRepository,
            ILogFactory logFactory)
        {
            _assetHedgeSettingsRepository = assetHedgeSettingsRepository;
            _log = logFactory.CreateLog(this);
            _cache = new InMemoryCache<AssetHedgeSettings>(GetKey, false);
        }

        public async Task<IReadOnlyCollection<AssetHedgeSettings>> GetAllAsync()
        {
            IReadOnlyCollection<AssetHedgeSettings> assetHedgeSettings = _cache.GetAll();

            if (assetHedgeSettings == null)
            {
                assetHedgeSettings = await _assetHedgeSettingsRepository.GetAllAsync();

                _cache.Initialize(assetHedgeSettings);
            }

            return assetHedgeSettings;
        }

        public async Task<AssetHedgeSettings> GetByAssetIdAsync(string assetId)
        {
            IReadOnlyCollection<AssetHedgeSettings> assetHedgeSettings = await GetAllAsync();

            return assetHedgeSettings.SingleOrDefault(o => o.AssetId == assetId);
        }

        public async Task<AssetHedgeSettings> EnsureAsync(string assetId)
        {
            AssetHedgeSettings assetHedgeSettings = await GetByAssetIdAsync(assetId);

            if (assetHedgeSettings == null)
            {
                assetHedgeSettings = AssetHedgeSettings.Create(assetId);

                await _assetHedgeSettingsRepository.InsertAsync(assetHedgeSettings);

                _cache.Set(assetHedgeSettings);

                _log.InfoWithDetails("Default asset hedge settings was added", assetHedgeSettings);
            }

            return assetHedgeSettings;
        }

        public async Task AddAsync(AssetHedgeSettings assetHedgeSettings)
        {
            AssetHedgeSettings currentAssetHedgeSettings = await GetByAssetIdAsync(assetHedgeSettings.AssetId);

            if (currentAssetHedgeSettings != null)
                throw new EntityAlreadyExistsException();

            assetHedgeSettings.Approve();

            await _assetHedgeSettingsRepository.InsertAsync(assetHedgeSettings);

            _cache.Set(assetHedgeSettings);

            _log.InfoWithDetails("Asset hedge settings was added", assetHedgeSettings);
        }

        public async Task UpdateAsync(AssetHedgeSettings assetHedgeSettings)
        {
            AssetHedgeSettings currentAssetHedgeSettings = await GetByAssetIdAsync(assetHedgeSettings.AssetId);

            if (currentAssetHedgeSettings == null)
                throw new EntityNotFoundException();

            currentAssetHedgeSettings.Update(assetHedgeSettings);
            
            currentAssetHedgeSettings.Approve();

            await _assetHedgeSettingsRepository.UpdateAsync(currentAssetHedgeSettings);

            _cache.Set(currentAssetHedgeSettings);

            _log.InfoWithDetails("Asset hedge settings was updated", currentAssetHedgeSettings);
        }

        public async Task DeleteAsync(string assetId)
        {
            AssetHedgeSettings currentAssetHedgeSettings = await GetByAssetIdAsync(assetId);

            if (currentAssetHedgeSettings == null)
                throw new EntityNotFoundException();

            await _assetHedgeSettingsRepository.DeleteAsync(assetId);

            _cache.Remove(GetKey(assetId));

            _log.InfoWithDetails("Asset hedge settings was removed", currentAssetHedgeSettings);
        }

        private static string GetKey(AssetHedgeSettings assetHedgeSettings)
            => GetKey(assetHedgeSettings.AssetId);

        private static string GetKey(string assetId)
            => assetId.ToUpper();
    }
}
