using System.Collections.Generic;
using System.Linq;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public static class CrossAssetPairPriceCalculator
    {
        /// <summary>
        /// Calculates price for a cross asset pair.
        /// Tries to find 2 asset pairs - IndexToken/USD and (QuoteAsset/USD or USD/QuoteAsset)
        /// and calculate new price.
        /// </summary>
        /// <param name="crossAssetPairSettings">Settings for resulting asset pair</param>
        /// <param name="indexPrices">Actual prices of all the indices</param>
        /// <param name="allAssetPairSettings">All available asset pairs settings</param>
        /// <param name="allQuotes">All available quotes</param>
        /// <param name="errorMessage">Error message if there was any</param>
        /// <returns>Resulting price or null, if there was an error</returns>
        public static decimal? Calculate(
            CrossAssetPairSettings crossAssetPairSettings,
            IReadOnlyCollection<IndexPrice> indexPrices,
            IReadOnlyCollection<AssetPairSettings> allAssetPairSettings,
            IReadOnlyCollection<Quote> allQuotes,
            out string errorMessage)
        {
            var baseAsset = crossAssetPairSettings.BaseAsset;
            var quoteAsset = crossAssetPairSettings.QuoteAsset;
            var usd = "USD";

            AssetPairSettings resultingAssetPairSettings = allAssetPairSettings
                .FirstOrDefault(x => x.BaseAsset == baseAsset && x.QuoteAsset == quoteAsset);

            if (resultingAssetPairSettings == null)
            {
                errorMessage = $"Can't find {baseAsset}/{quoteAsset} asset pair settings.";

                return null;
            }

            // base asset is always an index token

            IndexPrice baseIndexTokenPrice = indexPrices.FirstOrDefault(x => x.Name == baseAsset);

            if (baseIndexTokenPrice == null)
            {
                errorMessage = $"Can't find {baseAsset} index price.";

                return null;
            }
            
            // if quote asset also is an index token, then get its price

            IndexPrice quoteIndexTokenPrice = indexPrices.FirstOrDefault(x => x.Name == baseAsset);

            decimal result;

            if (quoteIndexTokenPrice != null)
            {
                if (quoteIndexTokenPrice.Price <= 0)
                {
                    errorMessage = $"{quoteIndexTokenPrice.Name} price value is {quoteIndexTokenPrice.Price}.";

                    return null;
                }

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
                {
                    errorMessage = $"Found {assetPairsWithQuoteAsset.Count} {quoteAsset}/USD or USD/{quoteAsset} asset pairs settings.";

                    return null;
                }

                AssetPairSettings assetPairWithQuoteAsset = assetPairsWithQuoteAsset.First();

                // getting quotes

                Quote quoteTickPrice = allQuotes.FirstOrDefault(x => x.AssetPairId == assetPairWithQuoteAsset.AssetPairId);

                if (quoteTickPrice == null)
                {
                    errorMessage = $"Found {assetPairsWithQuoteAsset.Count} {quoteAsset}/USD or USD/{quoteAsset} quotes.";

                    return null;
                }

                decimal quoteIndexTokenPriceValue = quoteTickPrice.Mid;

                if (quoteIndexTokenPriceValue <= 0)
                {
                    errorMessage = $"{quoteTickPrice.AssetPairId} mid price is {quoteIndexTokenPriceValue}.";

                    return null;
                }

                // calculating price
                
                if (assetPairWithQuoteAsset.BaseAsset == usd)
                    // straight second pair
                    result = baseIndexTokenPrice.Price * quoteIndexTokenPriceValue;
                else
                    // reversed second pair
                    result = baseIndexTokenPrice.Price / quoteIndexTokenPriceValue;
            }

            errorMessage = null;

            return result;
        }
    }
}
