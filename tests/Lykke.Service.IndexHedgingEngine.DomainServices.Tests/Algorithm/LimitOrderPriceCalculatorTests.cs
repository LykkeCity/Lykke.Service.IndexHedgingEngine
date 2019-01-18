using System;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Tests.Algorithm
{
    [TestClass]
    public class LimitOrderPriceCalculatorTests
    {
        [TestMethod]
        public void Calculate_Sell_Limit_Price()
        {
            // arrange

            var quote = new Quote("BTCUSD", DateTime.UtcNow, 6000, 5000, "source");

            decimal volume = 2000;

            LimitOrderType limitOrderType = LimitOrderType.Sell;

            var hedgeSettings = new HedgeSettings
            {
                ThresholdUp = 5000,
                ThresholdDown = 1000,
                MarketOrderMarkup = .1m
            };
            
            // act

            LimitOrderPrice limitOrderPrice =
                LimitOrderPriceCalculator.Calculate(quote, volume, limitOrderType, hedgeSettings);

            // assert

            Assert.IsTrue(limitOrderPrice.Price == quote.Ask && limitOrderPrice.Type == PriceType.Limit);
        }
        
        [TestMethod]
        public void Calculate_Buy_Limit_Price()
        {
            // arrange

            var quote = new Quote("BTCUSD", DateTime.UtcNow, 6000, 5000, "source");

            decimal volume = 2000;

            LimitOrderType limitOrderType = LimitOrderType.Buy;

            var hedgeSettings = new HedgeSettings
            {
                ThresholdUp = 5000,
                ThresholdDown = 1000,
                MarketOrderMarkup = .1m
            };
            
            // act

            LimitOrderPrice limitOrderPrice =
                LimitOrderPriceCalculator.Calculate(quote, volume, limitOrderType, hedgeSettings);

            // assert

            Assert.IsTrue(limitOrderPrice.Price == quote.Bid && limitOrderPrice.Type == PriceType.Limit);
        }
        
        [TestMethod]
        public void Calculate_Sell_Market_Price()
        {
            // arrange

            var quote = new Quote("BTCUSD", DateTime.UtcNow, 6000, 5000, "source");

            decimal volume = 6000;

            LimitOrderType limitOrderType = LimitOrderType.Sell;

            var hedgeSettings = new HedgeSettings
            {
                ThresholdUp = 5000,
                ThresholdDown = 1000,
                MarketOrderMarkup = .1m
            };
            
            // act

            LimitOrderPrice limitOrderPrice =
                LimitOrderPriceCalculator.Calculate(quote, volume, limitOrderType, hedgeSettings);

            // assert

            Assert.IsTrue(limitOrderPrice.Price == quote.Bid * (1 - hedgeSettings.MarketOrderMarkup) &&
                          limitOrderPrice.Type == PriceType.Market);
        }
        
        [TestMethod]
        public void Calculate_Buy_Market_Price()
        {
            // arrange

            var quote = new Quote("BTCUSD", DateTime.UtcNow, 6000, 5000, "source");

            decimal volume = 6000;

            LimitOrderType limitOrderType = LimitOrderType.Buy;

            var hedgeSettings = new HedgeSettings
            {
                ThresholdUp = 5000,
                ThresholdDown = 1000,
                MarketOrderMarkup = .1m
            };
            
            // act

            LimitOrderPrice limitOrderPrice =
                LimitOrderPriceCalculator.Calculate(quote, volume, limitOrderType, hedgeSettings);

            // assert

            Assert.IsTrue(limitOrderPrice.Price == quote.Ask * (1 + hedgeSettings.MarketOrderMarkup) &&
                          limitOrderPrice.Type == PriceType.Market);
        }
    }
}
