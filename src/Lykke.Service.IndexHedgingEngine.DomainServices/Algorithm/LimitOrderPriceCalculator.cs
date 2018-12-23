using System;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public static class LimitOrderPriceCalculator
    {
        public static LimitOrderPrice Calculate(Quote quote, decimal volume, LimitOrderType limitOrderType,
            HedgeSettings hedgeSettings)
        {
            if (hedgeSettings.ThresholdDown < volume && volume < hedgeSettings.ThresholdUp)
            {
                decimal price = limitOrderType == LimitOrderType.Sell
                    ? quote.Ask
                    : quote.Bid;

                return new LimitOrderPrice(price, PriceType.Limit);
            }

            if (hedgeSettings.ThresholdUp <= volume)
            {
                decimal price = limitOrderType == LimitOrderType.Sell
                    ? quote.Bid * (1 - hedgeSettings.MarketOrderMarkup)
                    : quote.Ask * (1 + hedgeSettings.MarketOrderMarkup);

                return new LimitOrderPrice(price, PriceType.Market);
            }

            throw new InvalidOperationException("The volume is out of the range");
        }

        public static bool CanCalculate(decimal volume, HedgeSettings hedgeSettings)
            => volume > hedgeSettings.ThresholdDown;
    }
}
