using System.Collections.Generic;
using System.Linq;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public static class InvestmentCalculator
    {
        public static IReadOnlyCollection<AssetInvestment> Calculate(
            IEnumerable<string> assets,
            IReadOnlyCollection<IndexSettings> indicesSettings,
            IReadOnlyCollection<Token> tokens,
            IReadOnlyCollection<IndexPrice> indexPrices,
            IReadOnlyCollection<Position> positions,
            IReadOnlyDictionary<string, Quote> assetPrices)
        {
            var assetsInvestments = new List<AssetInvestment>();

            foreach (string assetId in assets)
            {
                var assetIndexInvestments = new List<AssetIndexInvestment>();

                bool isDisabled = false;

                foreach (IndexSettings indexSettings in indicesSettings)
                {
                    Token token = tokens.SingleOrDefault(o => o.AssetId == indexSettings.AssetId);

                    IndexPrice indexPrice = indexPrices.Single(o => o.Name == indexSettings.Name);

                    AssetWeight assetWeight = indexPrice.Weights.SingleOrDefault(o => o.AssetId == assetId);

                    decimal weight = assetWeight?.Weight ?? 0;

                    decimal openVolume = token?.OpenVolume ?? 0;

                    decimal amount = openVolume * indexPrice.Price * weight;

                    isDisabled = isDisabled || (assetWeight?.IsDisabled ?? false);

                    assetIndexInvestments.Add(new AssetIndexInvestment
                    {
                        Name = indexSettings.Name,
                        IsShort = indexSettings.IsShort,
                        Value = indexPrice.Value,
                        Price = indexPrice.Price,
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

                decimal totalLongAmount = assetIndexInvestments.Where(x => !x.IsShort).Sum(o => o.Amount);

                decimal totalShortAmount = assetIndexInvestments.Where(x => x.IsShort).Sum(o => o.Amount);

                decimal totalAmount = totalLongAmount - totalShortAmount;

                decimal remainingAmount = assetVolume * assetPrice - totalAmount;

                assetsInvestments.Add(new AssetInvestment
                {
                    AssetId = assetId,
                    Volume = assetVolume,
                    Quote = quote,
                    TotalAmount = totalAmount,
                    RemainingAmount = remainingAmount,
                    IsDisabled = isDisabled,
                    Indices = assetIndexInvestments
                });
            }

            return assetsInvestments;
        }
    }
}
