using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settings
{
    public class AssetLinkService : IAssetLinkService
    {
        private readonly IAssetLinkRepository _assetLinkRepository;
        private readonly IAssetHedgeSettingsService _assetHedgeSettingsService;
        private readonly IAssetsReadModelRepository _assetsReadModelRepository;
        private readonly ILog _log;
        private readonly InMemoryCache<AssetLink> _cache;

        public AssetLinkService(
            IAssetLinkRepository assetLinkRepository,
            IAssetHedgeSettingsService assetHedgeSettingsService,
            IAssetsReadModelRepository assetsReadModelRepository,
            ILogFactory logFactory)
        {
            _assetLinkRepository = assetLinkRepository;
            _assetHedgeSettingsService = assetHedgeSettingsService;
            _assetsReadModelRepository = assetsReadModelRepository;
            _log = logFactory.CreateLog(this);
            _cache = new InMemoryCache<AssetLink>(GetKey, false);
        }

        public async Task<IReadOnlyCollection<AssetLink>> GetAllAsync()
        {
            IReadOnlyCollection<AssetLink> assetLinks = _cache.GetAll();

            if (assetLinks == null)
            {
                assetLinks = await _assetLinkRepository.GetAllAsync();

                _cache.Initialize(assetLinks);
            }

            return assetLinks;
        }

        public async Task<AssetLink> GetByAssetIdAsync(string assetId)
        {
            IReadOnlyCollection<AssetLink> assetLinks = await GetAllAsync();

            return assetLinks.SingleOrDefault(o => o.AssetId == assetId);
        }

        public async Task<IReadOnlyCollection<string>> GetMissedAsync()
        {
            IReadOnlyCollection<AssetLink> assetLinks = await GetAllAsync();

            IReadOnlyCollection<AssetHedgeSettings> assetHedgeSettings =
                await _assetHedgeSettingsService.GetAllAsync();

            return assetHedgeSettings
                .Where(o => o.Exchange == ExchangeNames.Lykke)
                .Where(o => assetLinks.All(p => p.AssetId != o.AssetId))
                .Where(o => _assetsReadModelRepository.TryGet(o.AssetId) == null)
                .Select(o => o.AssetId)
                .ToArray();
        }

        public async Task AddAsync(AssetLink assetLink)
        {
            AssetLink currentAssetLink = await GetByAssetIdAsync(assetLink.AssetId);

            if (currentAssetLink != null)
                throw new EntityAlreadyExistsException();

            await _assetLinkRepository.InsertAsync(assetLink);

            _cache.Set(assetLink);

            _log.InfoWithDetails("Asset link was added", assetLink);
        }

        public async Task UpdateAsync(AssetLink assetLink)
        {
            AssetLink currentAssetLink = await GetByAssetIdAsync(assetLink.AssetId);

            if (currentAssetLink == null)
                throw new EntityNotFoundException();

            currentAssetLink.Update(assetLink);

            await _assetLinkRepository.UpdateAsync(currentAssetLink);

            _cache.Set(currentAssetLink);

            _log.InfoWithDetails("Asset link was updated", currentAssetLink);
        }

        public async Task DeleteAsync(string assetId)
        {
            AssetLink currentAssetLink = await GetByAssetIdAsync(assetId);

            if (currentAssetLink == null)
                throw new EntityNotFoundException();

            await _assetLinkRepository.DeleteAsync(assetId);

            _cache.Remove(GetKey(assetId));

            _log.InfoWithDetails("Asset link was removed", currentAssetLink);
        }

        private static string GetKey(AssetLink assetLink)
            => GetKey(assetLink.AssetId);

        private static string GetKey(string assetId)
            => assetId.ToUpper();
    }
}
