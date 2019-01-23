using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public static class LimitOrderPriceCalculator
    {
        public static LimitOrderPrice Calculate(Quote quote, decimal volume, LimitOrderType limitOrderType,
            decimal thresholdUp, decimal marketOrderMarkup)
        {
            if (volume < thresholdUp)
            {
                decimal price = limitOrderType == LimitOrderType.Sell
                    ? quote.Ask
                    : quote.Bid;

                return new LimitOrderPrice(price, PriceType.Limit);
            }
            else
            {
                decimal price = limitOrderType == LimitOrderType.Sell
                    ? quote.Bid * (1 -marketOrderMarkup)
                    : quote.Ask * (1 +marketOrderMarkup);

                return new LimitOrderPrice(price, PriceType.Market);
            }
        }
    }
}
