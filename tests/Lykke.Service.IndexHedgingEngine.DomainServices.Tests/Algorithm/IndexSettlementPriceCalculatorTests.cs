using System;
using Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Tests.Algorithm
{
    [TestClass]
    public class IndexSettlementPriceCalculatorTests
    {
        [TestMethod]
        public void Calculate_Expected_Price()
        {
            // arrange

            decimal currentIndexValue = 500;
            decimal previousIndexValue = 400;
            decimal alpha = .5m;
            decimal previousK = .001m;
            decimal previousPrice = 1000;
            DateTime currentIndexTimestamp = DateTime.UtcNow;
            DateTime previousIndexTimestamp = DateTime.UtcNow.AddMinutes(-1);
            decimal trackingFee = .2m;
            decimal performanceFee = .2m;

            var expectedIndexSettlementPrice = new IndexSettlementPrice(
                1249.9995719176186738968163368m,
                125.0005m,
                0.25m,
                0.0000019025875158548959918823m);

            // act

            IndexSettlementPrice actualIndexSettlementPrice = IndexSettlementPriceCalculator.Calculate(
                currentIndexValue,
                previousIndexValue,
                alpha,
                previousK,
                previousPrice,
                currentIndexTimestamp,
                previousIndexTimestamp,
                trackingFee,
                performanceFee);

            // assert

            Assert.IsTrue(AreEqual(expectedIndexSettlementPrice, actualIndexSettlementPrice),
                "Wrong index settlement price");
        }

        private static bool AreEqual(IndexSettlementPrice left, IndexSettlementPrice right)
            => left.Delta == right.Delta && left.K == right.K && left.R == right.R && left.Price == right.Price;
    }
}
