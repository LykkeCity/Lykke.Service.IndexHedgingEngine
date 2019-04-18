using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;
using Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Tests.Algorithm
{
    [TestClass]
    public class InvestmentCalculatorTests
    {
        [TestMethod]
        public void Calculate_Zero_Investments_And_Zero_Positions()
        {
            // arrange

            var assets = new[] {"BTC", "LTC", "XRP"};

            var indicesSettings = new[] {new IndexSettings {Name = "LCI", AssetId = "TLYCI"}};

            var tokens = new Token[0];

            var indexPrices = new[]
            {
                new IndexPrice
                {
                    Name = "LCI", Price = 100, Value = 101, Weights = new[]
                    {
                        new AssetWeight("BTC", .5m, 1, false),
                        new AssetWeight("LTC", .3m, 1, false),
                        new AssetWeight("XRP", .2m, 1, false)
                    }
                }
            };

            var positions = new Position[0];

            var assetPrice = new Dictionary<string, Quote>
            {
                ["BTC"] = new Quote("BTCUSD", DateTime.UtcNow, 4000, 3900, "source"),
                ["LTC"] = new Quote("LTCUSD", DateTime.UtcNow, 32, 31, "source"),
                ["XRP"] = new Quote("XRPUSD", DateTime.UtcNow, .4m, .3m, "source")
            };

            var expectedAssetInvestments = new[]
            {
                new AssetInvestment
                {
                    AssetId = "BTC", Volume = 0, TotalAmount = 0, RemainingAmount = 0,
                    Indices = new[] {new AssetIndexInvestment {Name = "LCI", Amount = 0}}
                },
                new AssetInvestment
                {
                    AssetId = "LTC", Volume = 0, TotalAmount = 0, RemainingAmount = 0,
                    Indices = new[] {new AssetIndexInvestment {Name = "LCI", Amount = 0}}
                },
                new AssetInvestment
                {
                    AssetId = "XRP", Volume = 0, TotalAmount = 0, RemainingAmount = 0,
                    Indices = new[] {new AssetIndexInvestment {Name = "LCI", Amount = 0}}
                }
            };

            Complete(expectedAssetInvestments, assetPrice, indicesSettings, indexPrices, tokens);

            // act

            IReadOnlyCollection<AssetInvestment> actualAssetInvestments =
                InvestmentCalculator.Calculate(assets, indicesSettings, tokens, indexPrices, positions, assetPrice);

            // assert

            Assert.IsTrue(AreEqual(expectedAssetInvestments, actualAssetInvestments));
        }

        [TestMethod]
        public void Calculate_With_Investments_And_Zero_Positions()
        {
            // arrange

            var assets = new[] {"BTC", "LTC", "XRP"};

            var indicesSettings = new[] {new IndexSettings {Name = "LCI", AssetId = "TLYCI"}};

            var tokens = new[] {new Token {AssetId = "TLYCI", OpenVolume = 10, OppositeVolume = 1000}};

            var indexPrices = new[]
            {
                new IndexPrice
                {
                    Name = "LCI", Price = 100, Value = 101, Weights = new[]
                    {
                        new AssetWeight("BTC", .5m, 1, false),
                        new AssetWeight("LTC", .3m, 1, false),
                        new AssetWeight("XRP", .2m, 1, false)
                    }
                }
            };

            var positions = new Position[0];

            var assetPrice = new Dictionary<string, Quote>
            {
                ["BTC"] = new Quote("BTCUSD", DateTime.UtcNow, 4000, 3900, "source"),
                ["LTC"] = new Quote("LTCUSD", DateTime.UtcNow, 32, 31, "source"),
                ["XRP"] = new Quote("XRPUSD", DateTime.UtcNow, .4m, .3m, "source")
            };

            var expectedAssetInvestments = new[]
            {
                new AssetInvestment
                {
                    AssetId = "BTC", Volume = 0, TotalAmount = 500, RemainingAmount = -500,
                    Indices = new[] {new AssetIndexInvestment {Name = "LCI", Amount = 500}}
                },
                new AssetInvestment
                {
                    AssetId = "LTC", Volume = 0, TotalAmount = 300, RemainingAmount = -300,
                    Indices = new[] {new AssetIndexInvestment {Name = "LCI", Amount = 300}}
                },
                new AssetInvestment
                {
                    AssetId = "XRP", Volume = 0, TotalAmount = 200, RemainingAmount = -200,
                    Indices = new[] {new AssetIndexInvestment {Name = "LCI", Amount = 200}}
                }
            };

            Complete(expectedAssetInvestments, assetPrice, indicesSettings, indexPrices, tokens);

            // act

            IReadOnlyCollection<AssetInvestment> actualAssetInvestments =
                InvestmentCalculator.Calculate(assets, indicesSettings, tokens, indexPrices, positions, assetPrice);

            // assert

            Assert.IsTrue(AreEqual(expectedAssetInvestments, actualAssetInvestments));
        }

        [TestMethod]
        public void Calculate_With_Investments_And_With_Positions()
        {
            // arrange

            var assets = new[] {"BTC", "LTC", "XRP"};

            var indicesSettings = new[] {new IndexSettings {Name = "LCI", AssetId = "TLYCI"}};

            var tokens = new[] {new Token {AssetId = "TLYCI", OpenVolume = 10, OppositeVolume = 1000}};

            var indexPrices = new[]
            {
                new IndexPrice
                {
                    Name = "LCI", Price = 100, Value = 101, Weights = new[]
                    {
                        new AssetWeight("BTC", .5m, 1, false),
                        new AssetWeight("LTC", .3m, 1, false),
                        new AssetWeight("XRP", .2m, 1, false)
                    }
                }
            };

            var positions = new[]
            {
                new Position {AssetId = "BTC", Volume = .05m},
                new Position {AssetId = "LTC", Volume = 142},
                new Position {AssetId = "XRP", Volume = 300}
            };

            var assetPrice = new Dictionary<string, Quote>
            {
                ["BTC"] = new Quote("BTCUSD", DateTime.UtcNow, 4000, 3900, "source"),
                ["LTC"] = new Quote("LTCUSD", DateTime.UtcNow, 32, 31, "source"),
                ["XRP"] = new Quote("XRPUSD", DateTime.UtcNow, .4m, .3m, "source")
            };

            var expectedAssetInvestments = new[]
            {
                new AssetInvestment
                {
                    AssetId = "BTC", Volume = .05m, TotalAmount = 500, RemainingAmount = -302.5m,
                    Indices = new[] {new AssetIndexInvestment {Name = "LCI", Amount = 500}}
                },
                new AssetInvestment
                {
                    AssetId = "LTC", Volume = 142, TotalAmount = 300, RemainingAmount = 4173,
                    Indices = new[] {new AssetIndexInvestment {Name = "LCI", Amount = 300}}
                },
                new AssetInvestment
                {
                    AssetId = "XRP", Volume = 300, TotalAmount = 200, RemainingAmount = -95,
                    Indices = new[] {new AssetIndexInvestment {Name = "LCI", Amount = 200}}
                }
            };

            Complete(expectedAssetInvestments, assetPrice, indicesSettings, indexPrices, tokens);

            // act

            IReadOnlyCollection<AssetInvestment> actualAssetInvestments =
                InvestmentCalculator.Calculate(assets, indicesSettings, tokens, indexPrices, positions, assetPrice);

            // assert

            Assert.IsTrue(AreEqual(expectedAssetInvestments, actualAssetInvestments));
        }
        
        private static void Complete(IReadOnlyCollection<AssetInvestment> assetInvestments,
            IReadOnlyDictionary<string, Quote> assetPrice, IReadOnlyCollection<IndexSettings> indexSettings,
            IReadOnlyCollection<IndexPrice> indexPrices, IReadOnlyCollection<Token> tokens)
        {
            foreach (AssetInvestment assetInvestment in assetInvestments)
            {
                assetInvestment.Quote = assetPrice[assetInvestment.AssetId];

                foreach (AssetIndexInvestment assetIndexInvestment in assetInvestment.Indices)
                {
                    IndexPrice indexPrice = indexPrices.Single(o => o.Name == assetIndexInvestment.Name);

                    IndexSettings settings = indexSettings.Single(p => p.Name == assetIndexInvestment.Name);

                    Token token = tokens.SingleOrDefault(t => t.AssetId == settings.AssetId);

                    assetIndexInvestment.Value = indexPrice.Value;
                    assetIndexInvestment.Price = indexPrice.Price;
                    assetIndexInvestment.OpenVolume = token?.OpenVolume ?? 0;
                    assetIndexInvestment.OppositeVolume = token?.OppositeVolume ?? 0;
                    assetIndexInvestment.Weight = indexPrice.Weights
                                                      .SingleOrDefault(o => o.AssetId == assetInvestment.AssetId)
                                                      ?.Weight ?? 0;
                }
            }
        }

        private static bool AreEqual(IReadOnlyCollection<AssetInvestment> left,
            IReadOnlyCollection<AssetInvestment> right)
        {
            if (left.Count != right.Count)
                return false;

            foreach (AssetInvestment leftAssetInvestment in left)
            {
                AssetInvestment rightAssetInvestment =
                    right.SingleOrDefault(o => o.AssetId == leftAssetInvestment.AssetId);

                if (rightAssetInvestment == null)
                    return false;

                if (!AreEqual(leftAssetInvestment, rightAssetInvestment))
                    return false;
            }

            return true;
        }

        private static bool AreEqual(AssetInvestment left, AssetInvestment right)
        {
            return left.AssetId == right.AssetId &&
                   left.Volume == right.Volume &&
                   left.TotalAmount == right.TotalAmount &&
                   left.RemainingAmount == right.RemainingAmount &&
                   left.IsDisabled == right.IsDisabled &&
                   AreEqual(left.Quote, right.Quote) &&
                   AreEqual(left.Indices, right.Indices);
        }

        private static bool AreEqual(IReadOnlyCollection<AssetIndexInvestment> left,
            IReadOnlyCollection<AssetIndexInvestment> right)
        {
            if (left.Count != right.Count)
                return false;

            foreach (AssetIndexInvestment leftAssetIndexInvestment in left)
            {
                AssetIndexInvestment rightAssetIndexInvestment =
                    right.SingleOrDefault(o => o.Name == leftAssetIndexInvestment.Name);

                if (rightAssetIndexInvestment == null)
                    return false;

                if (!AreEqual(leftAssetIndexInvestment, rightAssetIndexInvestment))
                    return false;
            }

            return true;
        }

        private static bool AreEqual(AssetIndexInvestment left, AssetIndexInvestment right)
        {
            return left.Name == right.Name &&
                   left.Value == right.Value &&
                   left.Price == right.Price &&
                   left.OpenVolume == right.OpenVolume &&
                   left.OppositeVolume == right.OppositeVolume &&
                   left.Weight == right.Weight &&
                   left.Amount == right.Amount;
        }

        private static bool AreEqual(Quote left, Quote right)
        {
            if (left != null && right == null || left == null && right != null)
                return false;

            if (left == null)
                return true;

            return left.Ask == right.Ask && left.Bid == right.Bid;
        }
    }
}
