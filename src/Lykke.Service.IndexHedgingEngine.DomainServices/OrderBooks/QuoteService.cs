using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.OrderBooks
{
    [UsedImplicitly]
    public class QuoteService : IQuoteService
    {
        private readonly InMemoryCache<Quote> _cache;

        public QuoteService()
        {
            _cache = new InMemoryCache<Quote>(GetKey, true);
        }

        public IReadOnlyCollection<Quote> GetAll()
        {
            return _cache.GetAll();
        }

        public Quote GetByAssetPairId(string source, string assetPairId)
        {
            if (source == ExchangeNames.Virtual)
            {
                Quote[] quotes = GetAll()
                    .Where(o => o.AssetPairId == assetPairId)
                    .ToArray();

                if (!quotes.Any())
                    return null;

                decimal avgMid = quotes
                    .Select(o => o.Mid)
                    .Average();

                return new Quote(assetPairId, quotes.Max(o => o.Time), avgMid, avgMid, ExchangeNames.Virtual);
            }

            return _cache.Get($"{source}-{assetPairId}");
        }

        public decimal GetAvgMid(string assetPairId)
        {
            return _cache.GetAll()
                .Where(o => o.AssetPairId == assetPairId)
                .Select(o => o.Mid)
                .DefaultIfEmpty(0)
                .Average();
        }

        public void Update(Quote quote)
        {
            _cache.Set(quote);
        }

        private static string GetKey(Quote quote)
            => GetKey(quote.Source, quote.AssetPairId);

        private static string GetKey(string source, string assetPairId)
            => $"{source}-{assetPairId}";
    }
}
