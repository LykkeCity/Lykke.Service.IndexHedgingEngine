using System;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public static class IndexSettlementPriceCalculator
    {
        public static IndexSettlementPrice Calculate(
            decimal currentIndexValue,
            decimal previousIndexValue,
            decimal alpha,
            decimal previousK,
            decimal previousPrice,
            DateTime currentIndexTimestamp,
            DateTime previousIndexTimestamp,
            decimal trackingFee,
            decimal performanceFee,
            bool isShort)
        {
            decimal r = currentIndexValue / previousIndexValue - 1;

            decimal k = alpha * previousK + (1 - alpha) * r * previousPrice;

            int currentYear = DateTime.UtcNow.Year;

            int daysInCurrentYear = (new DateTime(currentYear + 1, 1, 1) - new DateTime(currentYear, 1, 1)).Days;

            decimal secondsSinceLastIndex = (decimal) (currentIndexTimestamp - previousIndexTimestamp).TotalSeconds;

            decimal delta = secondsSinceLastIndex / (daysInCurrentYear * 24 * 60 * 60);

            decimal rTrackingFeeAndDelta = r - trackingFee * delta;

            if (!isShort)
                rTrackingFeeAndDelta = 1 + rTrackingFeeAndDelta;
            else
                rTrackingFeeAndDelta = 1 - rTrackingFeeAndDelta;

            decimal price = previousPrice * rTrackingFeeAndDelta - performanceFee * delta * Math.Max(0, k);

            return new IndexSettlementPrice(price, k, r, delta);
        }
    }
}
