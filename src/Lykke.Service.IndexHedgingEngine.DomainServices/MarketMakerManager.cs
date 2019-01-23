using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
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
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleIndexAsync(Index index)
        {
            MarketMakerState marketMakerState = await _marketMakerStateService.GetAsync();

            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(index.Name);
            
            if (marketMakerState.Status != MarketMakerStatus.Active)
            {
                if (indexSettings != null)
                {
                    foreach (AssetWeight assetWeight in index.Weights)
                        await _assetHedgeSettingsService.EnsureAsync(assetWeight.AssetId);                        
                }
                
                return;
            }
            
            if(indexSettings == null)
                return;
            
            await _semaphore.WaitAsync();

            try
            {
                await _indexPriceService.UpdateAsync(index);

                await _hedgeService.UpdateLimitOrdersAsync();

                await _marketMakerService.UpdateLimitOrdersAsync(index.Name);
            }
            catch (InvalidOperationException exception)
            {
                _log.WarningWithDetails(exception.Message, index);
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

                foreach (InternalTrade internalTrade in internalTrades)
                {
                    IndexSettings indexSettings = indicesSettings
                        .SingleOrDefault(o => o.AssetPairId == internalTrade.AssetPairId);

                    if (indexSettings != null)
                    {
                        await _internalTradeService.RegisterAsync(internalTrade);

                        await _tokenService.UpdateVolumeAsync(indexSettings.AssetId, internalTrade);
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task HandleMarketMakerStateAsync(MarketMakerStatus marketMakerStatus, string comment,
            string userId)
        {
            await _marketMakerStateService.UpdateAsync(marketMakerStatus, comment, userId);

            if (marketMakerStatus != MarketMakerStatus.Active)
            {
                IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

                foreach (IndexSettings indexSettings in indicesSettings)
                    await _marketMakerService.CancelLimitOrdersAsync(indexSettings.Name);
            }
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
    }
}
