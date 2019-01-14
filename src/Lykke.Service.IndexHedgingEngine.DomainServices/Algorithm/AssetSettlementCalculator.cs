using System;
using System.Collections.Generic;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public static class AssetSettlementCalculator
    {
        public static IReadOnlyCollection<AssetSettlementAmount> Calculate(decimal amount, decimal price,
            IReadOnlyDictionary<string, decimal> assetWeight, IReadOnlyDictionary<string, Quote> assetPrices)
        {
            var assetAmounts = new List<AssetSettlementAmount>();
             
            decimal amountInUsd = amount * price;

            foreach (KeyValuePair<string,decimal> pair in assetWeight)
            {
                if (!assetPrices.TryGetValue(pair.Key, out Quote quote))
                    throw new InvalidOperationException($"No quote for asset '{pair.Key}'");
                
                decimal assetAmountInUsd = amountInUsd * pair.Value;

                decimal assetAmount = assetAmountInUsd / quote.Mid;

                assetAmounts.Add(new AssetSettlementAmount(pair.Key, assetAmount, quote.Mid, pair.Value));
            }

            return assetAmounts;
        }
    }
}
