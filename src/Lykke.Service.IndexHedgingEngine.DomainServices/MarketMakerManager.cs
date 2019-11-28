using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Handlers;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices
{
    public class MarketMakerManager : IIndexHandler, IInternalTradeHandler, IMarketMakerStateHandler, ISettlementHandler
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly IIndexPriceService _indexPriceService;
        private readonly IMarketMakerService _marketMakerService;
        private readonly IHedgeService _hedgeService;
        private readonly IInternalTradeService _internalTradeService;
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IAssetHedgeSettingsService _assetHedgeSettingsService;
        private readonly ITokenService _tokenService;
        private readonly IMarketMakerStateService _marketMakerStateService;
        private readonly ISettlementService _settlementService;
        private readonly IQuoteService _quoteService;
        private readonly ICrossAssetPairSettingsService _crossAssetPairSettingsService;
        private readonly ILog _log;

        public MarketMakerManager(
            IIndexPriceService indexPriceService,
            IMarketMakerService marketMakerService,
            IHedgeService hedgeService,
            IInternalTradeService internalTradeService,
            IIndexSettingsService indexSettingsService,
            IAssetHedgeSettingsService assetHedgeSettingsService,
            ITokenService tokenService,
            IMarketMakerStateService marketMakerStateService,
            ISettlementService settlementService,
            IQuoteService quoteService,
            ICrossAssetPairSettingsService crossAssetPairSettingsService,
            ILogFactory logFactory)
        {
            _indexPriceService = indexPriceService;
            _marketMakerService = marketMakerService;
            _hedgeService = hedgeService;
            _internalTradeService = internalTradeService;
            _indexSettingsService = indexSettingsService;
            _assetHedgeSettingsService = assetHedgeSettingsService;
            _tokenService = tokenService;
            _marketMakerStateService = marketMakerStateService;
            _settlementService = settlementService;
            _quoteService = quoteService;
            _crossAssetPairSettingsService = crossAssetPairSettingsService;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleIndexAsync(Index index, Index shortIndex)
        {
            MarketMakerState marketMakerState = await _marketMakerStateService.GetAsync();

            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(index.Name);

            IndexSettings shortIndexSettings = null;

            if (shortIndex != null)
                shortIndexSettings = await _indexSettingsService.GetByIndexAsync(shortIndex.Name);

            if (shortIndexSettings == null)
                shortIndex = null;

            if (marketMakerState.Status != MarketMakerStatus.Active)
            {
                await UpdateAssetsAsync(indexSettings, index);

                return;
            }
            
            await UpdateVirtualExchangePricesAsync(index);

            if (indexSettings == null)
                return;
            
            await _semaphore.WaitAsync();

            try
            {
                await UpdateIndicesPricesAsync(index, shortIndex);

                await _hedgeService.UpdateLimitOrdersAsync();

                await UpdateMarketMakerOrdersAsync(index, shortIndex);
            }
            catch (InvalidOperationException exception)
            {
                _log.WarningWithDetails("An error occurred while processing index", exception, index);
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, "An error occurred while processing index", index);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task HandleInternalTradesAsync(IReadOnlyCollection<InternalTrade> internalTrades)
        {
            await _semaphore.WaitAsync();

            try
            {
                IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

                IReadOnlyCollection<CrossAssetPairSettings> crossAssetPairsSettings = await _crossAssetPairSettingsService.GetAllAsync();

                foreach (InternalTrade internalTrade in internalTrades)
                {
                    IndexSettings indexSettings = indicesSettings
                        .SingleOrDefault(o => o.AssetPairId == internalTrade.AssetPairId);

                    if (indexSettings != null)
                    {
                        await _internalTradeService.RegisterAsync(internalTrade);

                        await _tokenService.UpdateVolumeAsync(indexSettings.AssetId, internalTrade);
                    }

                    CrossAssetPairSettings crossAssetPairSettings = crossAssetPairsSettings
                        .SingleOrDefault(x => x.AssetPairId == internalTrade.AssetPairId);

                    if (crossAssetPairSettings != null)
                    {
                        await _internalTradeService.RegisterAsync(internalTrade);

                        await _tokenService.UpdateVolumeCrossPairAsync(crossAssetPairSettings.BaseAssetId, crossAssetPairSettings.QuoteAssetId, internalTrade);
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task HandleMarketMakerStateAsync(MarketMakerStatus marketMakerStatus, string comment, string userId)
        {
            await _marketMakerStateService.UpdateAsync(marketMakerStatus, comment, userId);

            if (marketMakerStatus != MarketMakerStatus.Active)
            {
                IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

                foreach (IndexSettings indexSettings in indicesSettings)
                    await _marketMakerService.CancelLimitOrdersAsync(indexSettings.Name);
            }
        }

        public async Task HandleCrossAssetPairStateAsync(Guid id, CrossAssetPairSettingsMode mode, string userId)
        {
            await _crossAssetPairSettingsService.UpdateModeAsync(id, mode, userId);

            CrossAssetPairSettings crossAssetPairSettings = await _crossAssetPairSettingsService.FindByIdAsync(id);

            if (mode == CrossAssetPairSettingsMode.Enabled)
                await _marketMakerService.UpdateCrossPairLimitOrdersAsync(crossAssetPairSettings);
            else
                await _marketMakerService.CancelCrossPairLimitOrdersAsync(crossAssetPairSettings);
        }

        public async Task ExecuteAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                await _settlementService.ExecuteAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task UpdateAssetsAsync(IndexSettings indexSettings, Index index)
        {
            if (indexSettings != null)
            {
                foreach (AssetWeight assetWeight in index.Weights)
                    await _assetHedgeSettingsService.EnsureAsync(assetWeight.AssetId);
            }
        }

        private async Task UpdateVirtualExchangePricesAsync(Index index)
        {
            foreach (var assetWeight in index.Weights)
            {
                Quote quote = new Quote($"{assetWeight.AssetId}USD", index.Timestamp, assetWeight.Price,
                    assetWeight.Price, ExchangeNames.Virtual);

                await _quoteService.UpdateAsync(quote);
            }
        }

        private async Task UpdateIndicesPricesAsync(Index index, Index shortIndex)
        {
            var indexPriceTasks = new List<Task>();

            indexPriceTasks.Add(_indexPriceService.UpdateAsync(index));

            if (shortIndex != null)
                indexPriceTasks.Add(_indexPriceService.UpdateAsync(shortIndex));

            await Task.WhenAll(indexPriceTasks);
        }

        private async Task UpdateMarketMakerOrdersAsync(Index index, Index shortIndex)
        {
            var updateLimitOrdersTasks = new List<Task>();

            updateLimitOrdersTasks.Add(_marketMakerService.UpdateLimitOrdersAsync(index.Name));

            if (shortIndex != null)
            {
                updateLimitOrdersTasks.Add(_marketMakerService.UpdateLimitOrdersAsync(shortIndex.Name));

                // commented out before the first release to dev env (not sure about logic here: TokenService -> UpdateVolumeCrossPairAsync)
                // await UpdateCrossPairsMarketMakerOrdersAsync(updateLimitOrdersTasks, index, shortIndex);
            }

            await Task.WhenAll(updateLimitOrdersTasks);
        }

        private async Task UpdateCrossPairsMarketMakerOrdersAsync(List<Task> updateLimitOrdersTasks, Index index,
            Index shortIndex)
        {
            var crossPairsToUpdate = await _crossAssetPairSettingsService.FindEnabledByIndexAsync(index.Name, shortIndex?.Name);

            foreach (var crossAssetPairSettings in crossPairsToUpdate)
                updateLimitOrdersTasks.Add(_marketMakerService.UpdateCrossPairLimitOrdersAsync(crossAssetPairSettings));
        }
    }
}
