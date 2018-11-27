using System;
using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public static class LimitOrderPriceCalculator
    {
        public static LimitOrderPrice CalculatePrice(
            decimal currentIndexValue,
            decimal previousIndexValue,
            decimal alpha,
            decimal previousK,
            decimal previousPrice,
            DateTime currentIndexTimestamp,
            DateTime previousIndexTimestamp,
            decimal trackingFee,
            decimal performanceFee)
        {
            decimal r = currentIndexValue / previousIndexValue - 1;

            decimal k = alpha * previousK + (1 - alpha) * r * previousPrice;

            int currentYear = DateTime.UtcNow.Year;

            int daysInCurrentYear = (new DateTime(currentYear + 1, 1, 1) - new DateTime(currentYear, 1, 1)).Days;

            decimal secondsSinceLastIndex = (decimal) (currentIndexTimestamp - previousIndexTimestamp).TotalSeconds;

            decimal delta = secondsSinceLastIndex / (daysInCurrentYear * 24 * 60 * 60);

            decimal price = previousPrice * (1 + r - trackingFee * delta) - performanceFee * delta * Math.Max(0, k);

            return new LimitOrderPrice(price, k, r, delta);
        }
    }
}
