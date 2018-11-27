using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settings
{
    public class MarketMakerStateService : IMarketMakerStateService
    {
        private const string CacheKey = "Key";

        private readonly IMarketMakerStateRepository _marketMakerStateRepository;
        private readonly ILog _log;
        private readonly InMemoryCache<MarketMakerState> _cache;

        public MarketMakerStateService(
            IMarketMakerStateRepository marketMakerStateRepository,
            ILogFactory logFactory)
        {
            _marketMakerStateRepository = marketMakerStateRepository;
            _log = logFactory.CreateLog(this);
            _cache = new InMemoryCache<MarketMakerState>(o => CacheKey, false);
        }

        public async Task<MarketMakerState> GetAsync()
        {
            MarketMakerState marketMakerState = _cache.Get(CacheKey);

            if (marketMakerState == null)
            {
                marketMakerState = await _marketMakerStateRepository.GetAsync();

                if (marketMakerState == null)
                {
                    marketMakerState = new MarketMakerState
                    {
                        Status = MarketMakerStatus.Stopped,
                        Timestamp = DateTime.UtcNow
                    };
                }

                _cache.Initialize(new[] {marketMakerState});
            }

            return marketMakerState;
        }

        public async Task UpdateAsync(MarketMakerStatus marketMakerStatus, string comment, string userId)
        {
            MarketMakerState marketMakerState = await GetAsync();

            marketMakerState.Status = marketMakerStatus;
            marketMakerState.Timestamp = DateTime.UtcNow;

            await _marketMakerStateRepository.InsertOrReplaceAsync(marketMakerState);

            _cache.Set(marketMakerState);

            _log.InfoWithDetails("Market maker status updated", new
            {
                marketMakerState,
                comment,
                userId
            });
        }
    }
}
