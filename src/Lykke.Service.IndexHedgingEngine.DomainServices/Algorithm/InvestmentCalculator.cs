using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public static class InvestmentCalculator
    {
        public static IReadOnlyCollection<AssetInvestment> CalculateInvestments(
            IEnumerable<string> assets,
            IReadOnlyCollection<IndexSettings> indicesSettings,
            IReadOnlyCollection<Token> tokens,
            IReadOnlyCollection<Index> indices,
            IReadOnlyCollection<IndexPrice> indexPrices,
            IReadOnlyCollection<Position> positions,
            IReadOnlyDictionary<string, Quote> assetPrices)
        {
            var assetsInvestments = new List<AssetInvestment>();

            foreach (string assetId in assets)
            {
                decimal assetInvestment = 0;

                var indexInvestments = new List<AssetIndexInvestment>();

                bool isDisabled = false;

                foreach (IndexSettings indexSettings in indicesSettings)
                {
                    Token token = tokens.SingleOrDefault(o => o.AssetId == indexSettings.AssetId);

                    Index index = indices.SingleOrDefault(o => o.Name == indexSettings.Name);

                    IndexPrice indexPrice = indexPrices.SingleOrDefault(o => o.Name == indexSettings.Name);

                    decimal weight = index?.Weights.SingleOrDefault(o => o.AssetId == assetId)?.Weight ?? 0;

                    decimal openVolume = token?.OpenVolume ?? 0;

                    decimal price = indexPrice?.Price ?? 0;

                    decimal amount = openVolume * price * weight;

                    assetInvestment += amount;

                    isDisabled = isDisabled ||
                                 (index?.Weights.SingleOrDefault(o => o.AssetId == assetId)?.IsDisabled ?? false);

                    indexInvestments.Add(new AssetIndexInvestment
                    {
                        Name = indexSettings.Name,
                        Value = index?.Value ?? 0,
                        Price = price,
                        OpenVolume = token?.OpenVolume ?? 0,
                        OppositeVolume = token?.OppositeVolume ?? 0,
                        Amount = amount,
                        Weight = weight
                    });
                }

                Position position = positions.SingleOrDefault(o => o.AssetId == assetId);

                decimal assetVolume = position?.Volume ?? 0;

                assetPrices.TryGetValue(assetId, out Quote quote);

                decimal assetPrice = quote?.Mid ?? 0;

                decimal remainingVolume = assetVolume * assetPrice - assetInvestment;

                assetsInvestments.Add(new AssetInvestment
                {
                    AssetId = assetId,
                    Volume = assetVolume,
                    Quote = quote,
                    TotalAmount = indexInvestments.Sum(o => o.Amount),
                    RemainingAmount = remainingVolume,
                    IsDisabled = isDisabled,
                    Indices = indexInvestments
                });
            }

            return assetsInvestments;
        }

        public static decimal CalculateHedgeLimitOrderPrice(Quote quote, decimal investments,
            HedgeSettings hedgeSettings, out PriceType priceType)
        {
            decimal absoluteInvestments = Math.Abs(investments);

            if (hedgeSettings.ThresholdDown < absoluteInvestments && absoluteInvestments < hedgeSettings.ThresholdUp)
            {
                priceType = PriceType.Limit;
                
                return investments > 0
                    ? quote.Ask
                    : quote.Bid;
            }

            if (hedgeSettings.ThresholdUp <= absoluteInvestments)
            {
                priceType = PriceType.Market;
                
                return investments > 0
                    ? quote.Bid * (1 - hedgeSettings.MarketOrderMarkup)
                    : quote.Ask * (1 + hedgeSettings.MarketOrderMarkup);
            }

            throw new InvalidOperationException("The investments are out of the range");
        }
    }
}
