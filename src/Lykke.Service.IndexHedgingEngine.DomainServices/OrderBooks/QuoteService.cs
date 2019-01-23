using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.OrderBooks
{
    [UsedImplicitly]
    public class QuoteService : IQuoteService
    {
        private readonly IQuoteThresholdSettingsService _quoteThresholdSettingsService;
        private readonly ILog _log;
        private readonly InMemoryCache<Quote> _cache;

        public QuoteService(
            IQuoteThresholdSettingsService quoteThresholdSettingsService,
            ILogFactory logFactory)
        {
            _quoteThresholdSettingsService = quoteThresholdSettingsService;
            _cache = new InMemoryCache<Quote>(GetKey, true);
            _log = logFactory.CreateLog(this);
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

        public async Task UpdateAsync(Quote quote)
        {
            Quote currentQuote = _cache.Get(GetKey(quote));

            if (currentQuote != null)
            {
                QuoteThresholdSettings quoteThresholdSettings = await _quoteThresholdSettingsService.GetAsync();

                if (quoteThresholdSettings.Enabled &&
                    Math.Abs(currentQuote.Mid - quote.Mid) / currentQuote.Mid > quoteThresholdSettings.Value)
                {
                    _log.WarningWithDetails("Invalid quote", new
                    {
                        Quote = quote,
                        CurrentQuote = currentQuote,
                        Threshold = quoteThresholdSettings.Value
                    });
                }
                else
                {
                    _cache.Set(quote);
                }
            }
            else
            {
                _cache.Set(quote);
            }
        }

        private static string GetKey(Quote quote)
            => GetKey(quote.Source, quote.AssetPairId);

        private static string GetKey(string source, string assetPairId)
            => $"{source}-{assetPairId}";
    }
}
