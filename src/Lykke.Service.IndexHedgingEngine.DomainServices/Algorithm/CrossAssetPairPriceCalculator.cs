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
        /// <param name="indicesPrices">Actual prices of all the indices</param>
        /// <param name="indicesSettings">Indices settings of all the indices</param>
        /// <param name="allAssetSettings">All asset settings</param>
        /// <param name="allAssetPairSettings">All available asset pairs settings</param>
        /// <param name="allQuotes">All available quotes</param>
        /// <param name="errorMessage">Error message if there was any</param>
        /// <returns>Resulting price or null, if there was an error</returns>
        public static decimal? Calculate(
            CrossAssetPairSettings crossAssetPairSettings,
            IReadOnlyCollection<IndexPrice> indicesPrices,
            IReadOnlyCollection<IndexSettings> indicesSettings,
            IReadOnlyCollection<AssetSettings> allAssetSettings,
            IReadOnlyCollection<AssetPairSettings> allAssetPairSettings,
            IReadOnlyCollection<Quote> allQuotes,
            out string errorMessage)
        {
            string usd = "USD";

            AssetPairSettings assetPairSettings = allAssetPairSettings
                .FirstOrDefault(x => x.AssetPairId == crossAssetPairSettings.AssetPairId);

            if (assetPairSettings == null)
            {
                errorMessage = $"Can't find asset pair settings for {crossAssetPairSettings.AssetPairId}.";

                return null;
            }

            AssetSettings baseAsset = allAssetSettings.SingleOrDefault(x => x.Asset == assetPairSettings.BaseAsset);

            if (baseAsset == null)
            {
                errorMessage = $"Can't find asset by Asset name = {assetPairSettings.BaseAsset}.";

                return null;
            }

            AssetSettings quoteAsset = allAssetSettings.SingleOrDefault(x => x.Asset == assetPairSettings.QuoteAsset);

            if (quoteAsset == null)
            {
                errorMessage = $"Can't find asset by Asset name = {assetPairSettings.QuoteAsset}.";

                return null;
            }

            // base asset is always an index token

            IndexSettings baseIndexSettings = indicesSettings.SingleOrDefault(x => x.AssetId == baseAsset.AssetId);

            if (baseIndexSettings == null)
            {
                errorMessage = $"Can't find index settings by AssetId = {baseAsset.AssetId}.";

                return null;
            }

            IndexPrice baseIndexTokenPrice = indicesPrices.FirstOrDefault(x => x.Name == baseIndexSettings.Name);

            if (baseIndexTokenPrice == null)
            {
                errorMessage = $"Can't find {baseIndexSettings.Name} index price.";

                return null;
            }

            // if quote asset also is an index token, then get its price

            IndexSettings quoteIndexSettings = indicesSettings.SingleOrDefault(x => x.AssetId == quoteAsset.AssetId);

            decimal result;

            if (quoteIndexSettings != null)
            {
                IndexPrice quoteIndexTokenPrice = indicesPrices.FirstOrDefault(x => x.Name == quoteIndexSettings.Name);

                if (quoteIndexTokenPrice == null)
                {
                    errorMessage = $"Can't find {quoteIndexSettings.Name} index price.";

                    return null;
                }

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

                IList<AssetPairSettings> assetPairsWithQuoteAsset = allAssetPairSettings.Where(x => x.BaseAsset == quoteAsset.AssetId
                                                                                            && x.QuoteAsset == usd).ToList();

                if (assetPairsWithQuoteAsset.Count == 0)
                    assetPairsWithQuoteAsset = allAssetPairSettings.Where(x => x.BaseAsset == usd 
                                                                            && x.QuoteAsset == quoteAsset.AssetId).ToList();

                if (assetPairsWithQuoteAsset.Count != 1)
                {
                    errorMessage = $"Found {assetPairsWithQuoteAsset.Count} {quoteAsset.Asset}/USD or USD/{quoteAsset.Asset} asset pairs settings.";

                    return null;
                }

                AssetPairSettings assetPairWithQuoteAsset = assetPairsWithQuoteAsset.First();

                // getting quotes

                Quote quoteTickPrice = allQuotes.FirstOrDefault(x => x.AssetPairId == assetPairWithQuoteAsset.AssetPairId);

                if (quoteTickPrice == null)
                {
                    errorMessage = $"Found {assetPairsWithQuoteAsset.Count} {quoteAsset.Asset}/USD or USD/{quoteAsset.Asset} quotes.";

                    return null;
                }

                decimal quoteIndexTokenPriceValue = quoteTickPrice.Mid;

                if (quoteIndexTokenPriceValue <= 0)
                {
                    errorMessage = $"Asset pair id is {quoteTickPrice.AssetPairId}, mid price is {quoteIndexTokenPriceValue}.";

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
