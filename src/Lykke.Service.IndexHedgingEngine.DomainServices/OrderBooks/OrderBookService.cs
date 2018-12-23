using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.OrderBooks
{
    public class OrderBookService : IOrderBookService
    {
        private readonly ISettingsService _settingsService;

        private readonly IIndexSettingsService _indexSettingsService;

        private readonly InMemoryCache<OrderBook> _cache;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly Dictionary<string, IReadOnlyCollection<LimitOrder>> _sellLimitOrders =
            new Dictionary<string, IReadOnlyCollection<LimitOrder>>();

        private readonly Dictionary<string, IReadOnlyCollection<LimitOrder>> _buyLimitOrders =
            new Dictionary<string, IReadOnlyCollection<LimitOrder>>();

        public OrderBookService(ISettingsService settingsService, IIndexSettingsService indexSettingsService)
        {
            _indexSettingsService = indexSettingsService;
            _settingsService = settingsService;
            _cache = new InMemoryCache<OrderBook>(GetKey, true);
        }

        public Task<IReadOnlyCollection<OrderBook>> GetAllAsync()
        {
            return Task.FromResult(_cache.GetAll());
        }

        public async Task<IReadOnlyCollection<OrderBook>> GetAsync(int limit)
        {
            string walletId = _settingsService.GetWalletId();

            var orderBooks = new List<OrderBook>();

            foreach (OrderBook orderBook in _cache.GetAll())
            {
                IEnumerable<LimitOrder> sellLimitOrders = orderBook.LimitOrders
                    .Where(o => o.Type == LimitOrderType.Sell && o.WalletId != walletId)
                    .Where(o => o.WalletId != walletId)
                    .Take(limit)
                    .Union(orderBook.LimitOrders.Where(o => o.Type == LimitOrderType.Sell && o.WalletId == walletId))
                    .OrderBy(o => o.Price);

                IEnumerable<LimitOrder> buyLimitOrders = orderBook.LimitOrders
                    .Where(o => o.Type == LimitOrderType.Buy && o.WalletId != walletId)
                    .Take(limit)
                    .Union(orderBook.LimitOrders.Where(o => o.Type == LimitOrderType.Buy && o.WalletId == walletId))
                    .OrderByDescending(o => o.Price);

                orderBooks.Add(new OrderBook
                {
                    AssetPairId = orderBook.AssetPairId,
                    Timestamp = orderBook.Timestamp,
                    LimitOrders = sellLimitOrders.Union(buyLimitOrders).ToArray()
                });
            }

            return orderBooks;
        }

        public async Task UpdateSellLimitOrdersAsync(string assetPairId, DateTime timestamp,
            IReadOnlyCollection<LimitOrder> limitOrders)
        {
            if (!await ValidateSettingsAsync(assetPairId))
                return;

            await _semaphore.WaitAsync();

            try
            {
                _sellLimitOrders[assetPairId] = limitOrders;

                await UpdateOrderBookAsync(assetPairId, timestamp);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateBuyLimitOrdersAsync(string assetPairId, DateTime timestamp,
            IReadOnlyCollection<LimitOrder> limitOrders)
        {
            if (!await ValidateSettingsAsync(assetPairId))
                return;

            await _semaphore.WaitAsync();

            try
            {
                _buyLimitOrders[assetPairId] = limitOrders;

                await UpdateOrderBookAsync(assetPairId, timestamp);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<bool> ValidateSettingsAsync(string assetPairId)
        {
            IReadOnlyCollection<IndexSettings> indexSettings = await _indexSettingsService.GetAllAsync();

            return indexSettings.Any(o => o.AssetPairId == assetPairId);
        }

        private Task UpdateOrderBookAsync(string assetPairId, DateTime timestamp)
        {
            if (!_sellLimitOrders.TryGetValue(assetPairId, out IReadOnlyCollection<LimitOrder> sellLimitOrders))
                return Task.CompletedTask;

            if (!_buyLimitOrders.TryGetValue(assetPairId, out IReadOnlyCollection<LimitOrder> buyLimitOrders))
                return Task.CompletedTask;

            bool isValid = true;

            if (sellLimitOrders.Any() && buyLimitOrders.Any())
                isValid = sellLimitOrders.Min(o => o.Price) > buyLimitOrders.Max(o => o.Price);

            if (!isValid)
                return Task.CompletedTask;

            _cache.Set(new OrderBook
            {
                AssetPairId = assetPairId,
                Timestamp = timestamp,
                LimitOrders = sellLimitOrders.Union(buyLimitOrders).ToArray()
            });

            return Task.CompletedTask;
        }

        private static string GetKey(OrderBook orderBook)
            => GetKey(orderBook.AssetPairId);

        private static string GetKey(string assetPairId)
            => assetPairId.ToUpper();
    }
}
