using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Utils
{
    public class ConvertingService : IConvertingService
    {
        private readonly IQuoteService _quoteService;
        private readonly IIndexPriceService _indexPriceService;
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IInstrumentService _instrumentService;

        public ConvertingService(IQuoteService quoteService,
            IIndexPriceService indexPriceService,
            IIndexSettingsService indexSettingsService,
            IInstrumentService instrumentService)
        {
            _quoteService = quoteService;
            _indexPriceService = indexPriceService;
            _indexSettingsService = indexSettingsService;
            _instrumentService = instrumentService;
        }

        public async Task<decimal?> ConvertToUsdAsync(string assetIdFrom, decimal value)
        {
            const string usd = "USD";
            const string lykke = "lykke";

            var fromAsset = await _instrumentService.GetAssetByIdAsync(assetIdFrom);

            var toAsset = await _instrumentService.GetAssetAsync(usd, lykke);

            var allAssetPairs = await _instrumentService.GetAssetPairsAsync();

            var allQuotes = _quoteService.GetAll().ToList();

            var indicesPrices = await _indexPriceService.GetAllAsync();

            // add quotes for every index to usd

            foreach (var indexPrice in indicesPrices)
            {
                var indexSettings = await _indexSettingsService.GetByIndexAsync(indexPrice.Name);

                var indexAsset = await _instrumentService.GetAssetByIdAsync(indexSettings.AssetId);

                var assetPair = allAssetPairs.SingleOrDefault(x => x.BaseAsset == indexAsset.Asset && x.QuoteAsset == usd);

                if (assetPair != null)
                    allQuotes.Add(new Quote(assetPair.AssetPairId, DateTime.Now, indexPrice.Price, indexPrice.Price,lykke));
            }

            var straightAssetPair =
                allAssetPairs.SingleOrDefault(x => x.BaseAsset == fromAsset.Asset && x.QuoteAsset == toAsset.Asset);

            decimal? result;

            if (straightAssetPair != null)
            {
                var straightQuote = _quoteService.GetByAssetPairId(straightAssetPair.AssetPairId);

                if (straightQuote != null)
                {
                    result = value * straightQuote.Mid;

                    return result.Value;
                }
            }

            var reversedAssetPair =
                allAssetPairs.SingleOrDefault(x => x.BaseAsset == toAsset.Asset && x.QuoteAsset == fromAsset.Asset);

            if (reversedAssetPair != null)
            {
                var reversedQuote = _quoteService.GetByAssetPairId(reversedAssetPair.AssetPairId);

                if (reversedQuote != null)
                {
                    result = value / reversedQuote.Mid;

                    return result.Value;
                }
            }

            return null;
        }
    }
}
