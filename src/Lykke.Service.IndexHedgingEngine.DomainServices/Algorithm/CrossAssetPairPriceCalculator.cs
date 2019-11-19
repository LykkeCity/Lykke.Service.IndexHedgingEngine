using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public static class CrossAssetPairPriceCalculator
    {
        /// <summary>
        /// Tries to find 2 asset pairs - IndexToken/USD and (QuoteAsset/USD or USD/QuoteAsset)
        /// and then calculate new price.
        /// </summary>
        /// <param name="crossAssetPairSettings">Settings for resulting asset pair</param>
        /// <param name="indexPrices">Actual prices of all the indices</param>
        /// <param name="allAssetPairSettings">All available asset pairs settings</param>
        /// <param name="allQuotes">All available quotes</param>
        /// <returns>Tuple with resulting price or null and an error if there was any</returns>
        public static Tuple<decimal?, string> Calculate(
            CrossAssetPairSettings crossAssetPairSettings,
            IReadOnlyCollection<IndexPrice> indexPrices,
            IReadOnlyCollection<AssetPairSettings> allAssetPairSettings,
            IReadOnlyCollection<Quote> allQuotes)
        {
            var baseAsset = crossAssetPairSettings.BaseAsset;
            var quoteAsset = crossAssetPairSettings.QuoteAsset;
            var usd = "USD";

            AssetPairSettings resultingAssetPairSettings = allAssetPairSettings
                .FirstOrDefault(x => x.BaseAsset == baseAsset && x.QuoteAsset == quoteAsset);

            if (resultingAssetPairSettings == null)
                return new Tuple<decimal?, string>(null, $"Can't find {baseAsset}/{quoteAsset} asset pair settings.");

            // base asset is always an index token

            IndexPrice baseIndexTokenPrice = indexPrices.FirstOrDefault(x => x.Name == baseAsset);

            if (baseIndexTokenPrice == null)
                return new Tuple<decimal?, string>(null, $"Can't find {baseAsset} index price.");

            // if quote asset also is an index token, then get its price

            IndexPrice quoteIndexTokenPrice = indexPrices.FirstOrDefault(x => x.Name == baseAsset);

            decimal result;

            if (quoteIndexTokenPrice != null)
            {
                if (quoteIndexTokenPrice.Price <= 0)
                    return new Tuple<decimal?, string>(null, $"Quote Index Token price value is {quoteIndexTokenPrice.Price}.");

                result = baseIndexTokenPrice.Price / quoteIndexTokenPrice.Price;
            }
            else
            {
                // looking for asset pair with quote asset

                IList<AssetPairSettings> assetPairsWithQuoteAsset = allAssetPairSettings.Where(x => x.BaseAsset == quoteAsset
                                                                                            && x.QuoteAsset == usd).ToList();

                if (assetPairsWithQuoteAsset.Count == 0)
                    assetPairsWithQuoteAsset = allAssetPairSettings.Where(x => x.BaseAsset == usd 
                                                                            && x.QuoteAsset == quoteAsset).ToList();

                if (assetPairsWithQuoteAsset.Count != 1)
                    return new Tuple<decimal?, string>(null, $"Found {assetPairsWithQuoteAsset.Count} {quoteAsset}/USD or USD/{quoteAsset} asset pairs.");

                AssetPairSettings assetPairWithQuoteAsset = assetPairsWithQuoteAsset.First();

                // getting quotes

                Quote quoteTickPrice = allQuotes.FirstOrDefault(x => x.AssetPairId == assetPairWithQuoteAsset.AssetPairId);

                if (quoteTickPrice == null)
                    return new Tuple<decimal?, string>(null, $"Found {assetPairsWithQuoteAsset.Count} {quoteAsset}/USD or USD/{quoteAsset} asset pairs.");

                decimal quoteIndexTokenPriceValue = quoteTickPrice.Mid;

                if (quoteIndexTokenPriceValue <= 0)
                    return new Tuple<decimal?, string>(null, $"Quote Index Token price value is {quoteIndexTokenPriceValue}.");

                // calculating price
                
                if (assetPairWithQuoteAsset.BaseAsset == usd)
                    // straight second pair
                    result = baseIndexTokenPrice.Price * quoteIndexTokenPriceValue;
                else
                    // reversed second pair
                    result = baseIndexTokenPrice.Price / quoteIndexTokenPriceValue;
            }

            return new Tuple<decimal?, string>(result, string.Empty);
        }
    }
}
