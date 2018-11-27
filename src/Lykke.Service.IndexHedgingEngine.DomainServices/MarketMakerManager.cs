using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Handlers;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices
{
    public class MarketMakerManager : IIndexHandler, IInternalTradeHandler
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly IMarketMakerService _marketMakerService;
        private readonly IHedgeService _hedgeService;
        private readonly IIndexService _indexService;
        private readonly IInternalTradeService _internalTradeService;
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly ITokenService _tokenService;

        public MarketMakerManager(
            IMarketMakerService marketMakerService,
            IHedgeService hedgeService,
            IIndexService indexService,
            IInternalTradeService internalTradeService,
            IIndexSettingsService indexSettingsService,
            ITokenService tokenService)
        {
            _marketMakerService = marketMakerService;
            _hedgeService = hedgeService;
            _indexService = indexService;
            _internalTradeService = internalTradeService;
            _indexSettingsService = indexSettingsService;
            _tokenService = tokenService;
        }

        public async Task HandleIndexAsync(Index index)
        {
            await _semaphore.WaitAsync();

            try
            {
                _indexService.Update(index);

                await _marketMakerService.UpdateOrderBookAsync(index);

                await _hedgeService.ExecuteAsync();
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
                bool updateRequired = false;

                IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

                foreach (InternalTrade internalTrade in internalTrades)
                {
                    IndexSettings indexSettings = indicesSettings
                        .SingleOrDefault(o => o.AssetPairId == internalTrade.AssetPairId);

                    if (indexSettings != null)
                    {
                        await _internalTradeService.RegisterAsync(internalTrade);

                        await _tokenService.UpdateVolumeAsync(indexSettings.AssetId, internalTrade.Type,
                            internalTrade.Volume, internalTrade.OppositeVolume);

                        updateRequired = true;
                    }
                }

                if (updateRequired)
                    await _hedgeService.ExecuteAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
