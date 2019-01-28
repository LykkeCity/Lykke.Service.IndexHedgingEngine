using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.OrderBooks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Tests.OrderBooks
{
    [TestClass]
    public class QuoteServiceTests
    {
        private readonly Mock<IQuoteThresholdSettingsService> _quoteThresholdSettingsServiceMock =
            new Mock<IQuoteThresholdSettingsService>();

        private readonly Mock<IInstrumentService> _instrumentServiceMock = new Mock<IInstrumentService>();

        private readonly QuoteThresholdSettings _quoteThresholdSettings = new QuoteThresholdSettings
        {
            Value = .2m,
            Enabled = true
        };

        private QuoteService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _quoteThresholdSettingsServiceMock.Setup(o => o.GetAsync())
                .Returns(() => Task.FromResult(_quoteThresholdSettings));

            _instrumentServiceMock.Setup(o => o.IsAssetPairExistAsync(It.IsAny<string>()))
                .Returns((string assetPair) => Task.FromResult(true));

            _service = new QuoteService(
                _quoteThresholdSettingsServiceMock.Object,
                _instrumentServiceMock.Object,
                EmptyLogFactory.Instance);
        }

        [TestMethod]
        public async Task Quote_Do_Not_Exceed_Threshold()
        {
            // arrange

            var firstQuote = new Quote("BTCUSD", DateTime.UtcNow.AddSeconds(-10), 6000, 5990, "lykke");

            decimal secondMid = firstQuote.Mid * (1 + _quoteThresholdSettings.Value - .1m);

            var secondQuote = new Quote("BTCUSD", DateTime.UtcNow, secondMid + 10, secondMid - 10, "lykke");

            // act

            await _service.UpdateAsync(firstQuote);

            await _service.UpdateAsync(secondQuote);

            Quote quote = _service.GetByAssetPairId("lykke", "BTCUSD");

            // assert

            Assert.IsTrue(secondQuote.Ask == quote.Ask && secondQuote.Bid == quote.Bid);
        }

        [TestMethod]
        public async Task Quote_Exceed_Threshold()
        {
            // arrange

            var firstQuote = new Quote("BTCUSD", DateTime.UtcNow.AddSeconds(-10), 6000, 5990, "lykke");

            decimal secondMid = firstQuote.Mid * (1 + _quoteThresholdSettings.Value + .1m);

            var secondQuote = new Quote("BTCUSD", DateTime.UtcNow, secondMid + 10, secondMid - 10, "lykke");

            // act

            await _service.UpdateAsync(firstQuote);

            await _service.UpdateAsync(secondQuote);

            Quote quote = _service.GetByAssetPairId("lykke", "BTCUSD");

            // assert

            Assert.IsTrue(firstQuote.Ask == quote.Ask && firstQuote.Bid == quote.Bid);
        }

        [TestMethod]
        public async Task Quote_Exceed_Threshold_Disabled()
        {
            // arrange

            _quoteThresholdSettings.Enabled = false;

            var firstQuote = new Quote("BTCUSD", DateTime.UtcNow.AddSeconds(-10), 6000, 5990, "lykke");

            decimal secondMid = firstQuote.Mid * (1 + _quoteThresholdSettings.Value + .1m);

            var secondQuote = new Quote("BTCUSD", DateTime.UtcNow, secondMid + 10, secondMid - 10, "lykke");

            // act

            await _service.UpdateAsync(firstQuote);

            await _service.UpdateAsync(secondQuote);

            Quote quote = _service.GetByAssetPairId("lykke", "BTCUSD");

            // assert

            Assert.IsTrue(secondQuote.Ask == quote.Ask && secondQuote.Bid == quote.Bid);
        }

        [TestMethod]
        public void Quote_Get_Avg_Price_From_0_Quotes()
        {
            // arrange

            const string assetPair = "BTCUSD";

            // act

            Quote actualQuote = _service.GetByAssetPairId(ExchangeNames.Virtual, assetPair);

            // assert

            Assert.IsNull(actualQuote);
        }

        [TestMethod]
        public async Task Quote_Get_Avg_Price_From_2_Quotes()
        {
            // arrange

            const string assetPair = "BTCUSD";

            var quotes = new[]
            {
                new Quote(assetPair, DateTime.UtcNow, 5800 * 1.01m, 5800 * .99m, "1"),
                new Quote(assetPair, DateTime.UtcNow, 6000 * 1.01m, 6000 * .99m, "3")
            };

            decimal expectedPrice = quotes
                .Average(o => o.Mid);

            // act

            foreach (Quote quote in quotes)
                await _service.UpdateAsync(quote);

            Quote actualQuote = _service.GetByAssetPairId(ExchangeNames.Virtual, assetPair);

            // assert

            Assert.AreEqual(expectedPrice, actualQuote.Mid);
        }

        [TestMethod]
        public async Task Quote_Get_Avg_Price_From_3_Quotes()
        {
            // arrange

            const string assetPair = "BTCUSD";

            var quotes = new[]
            {
                new Quote(assetPair, DateTime.UtcNow, 5800 * 1.01m, 5800 * .99m, "1"),
                new Quote(assetPair, DateTime.UtcNow, 5900 * 1.02m, 5900 * .98m, "2"),
                new Quote(assetPair, DateTime.UtcNow, 6000 * 1.01m, 6000 * .99m, "3")
            };

            decimal expectedPrice = quotes
                .OrderBy(o => o.Mid)
                .ToList()
                .GetRange(1, quotes.Length - 2)
                .Average(o => o.Mid);

            // act

            foreach (Quote quote in quotes)
                await _service.UpdateAsync(quote);

            Quote actualQuote = _service.GetByAssetPairId(ExchangeNames.Virtual, assetPair);

            // assert

            Assert.AreEqual(expectedPrice, actualQuote.Mid);
        }

        [TestMethod]
        public async Task Quote_Get_Avg_Price_From_4_Quotes()
        {
            // arrange

            const string assetPair = "BTCUSD";

            var quotes = new[]
            {
                new Quote(assetPair, DateTime.UtcNow, 5800 * 1.01m, 5800 * .99m, "1"),
                new Quote(assetPair, DateTime.UtcNow, 5900 * 1.02m, 5900 * .98m, "2"),
                new Quote(assetPair, DateTime.UtcNow, 6000 * 1.01m, 6000 * .99m, "3"),
                new Quote(assetPair, DateTime.UtcNow, 6100 * 1.04m, 6100 * .96m, "4")
            };

            decimal expectedPrice = quotes
                .OrderBy(o => o.Mid)
                .ToList()
                .GetRange(1, quotes.Length - 2)
                .Average(o => o.Mid);

            // act

            foreach (Quote quote in quotes)
                await _service.UpdateAsync(quote);

            Quote actualQuote = _service.GetByAssetPairId(ExchangeNames.Virtual, assetPair);

            // assert

            Assert.AreEqual(expectedPrice, actualQuote.Mid);
        }

        [TestMethod]
        public async Task Quote_Get_Avg_Price_From_7_Quotes()
        {
            // arrange

            const string assetPair = "BTCUSD";

            var quotes = new[]
            {
                new Quote(assetPair, DateTime.UtcNow, 5800 * 1.01m, 5800 * .99m, "1"),
                new Quote(assetPair, DateTime.UtcNow, 5900 * 1.02m, 5900 * .98m, "2"),
                new Quote(assetPair, DateTime.UtcNow, 6000 * 1.01m, 6000 * .99m, "3"),
                new Quote(assetPair, DateTime.UtcNow, 6100 * 1.04m, 6100 * .96m, "4"),
                new Quote(assetPair, DateTime.UtcNow, 6500 * 1.05m, 6500 * .95m, "5"),
                new Quote(assetPair, DateTime.UtcNow, 7000 * 1.03m, 7000 * .97m, "6"),
                new Quote(assetPair, DateTime.UtcNow, 7100 * 1.01m, 7100 * .99m, "7")
            };

            decimal expectedPrice = quotes
                .OrderBy(o => o.Mid)
                .ToList()
                .GetRange(2, quotes.Length - 4)
                .Average(o => o.Mid);

            // act

            foreach (Quote quote in quotes)
                await _service.UpdateAsync(quote);

            Quote actualQuote = _service.GetByAssetPairId(ExchangeNames.Virtual, assetPair);

            // assert

            Assert.AreEqual(expectedPrice, actualQuote.Mid);
        }
        
        [TestMethod]
        public async Task Do_Not_Update_Quote_If_No_Associated_Instrument()
        {
            // arrange

            _instrumentServiceMock.Setup(o => o.IsAssetPairExistAsync(It.IsAny<string>()))
                .Returns((string assetPair) => Task.FromResult(false));
            
            var expectedQuote = new Quote("BTCUSD", DateTime.UtcNow, 6000, 5990, "lykke");

            // act

            await _service.UpdateAsync(expectedQuote);

            Quote actualQuote = _service.GetByAssetPairId(expectedQuote.Source, expectedQuote.AssetPairId);

            // assert

            Assert.IsNull(actualQuote);
        }
    }
}
