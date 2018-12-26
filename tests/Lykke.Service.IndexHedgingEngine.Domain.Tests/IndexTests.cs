using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.IndexHedgingEngine.Domain.Tests
{
    [TestClass]
    public class IndexTests
    {
        [TestMethod]
        public void Index_Is_Not_Valid_If_Sum_Of_Assets_Weights_Are_Less_Than_0_9()
        {
            // arrange
            
            var index = new Index("test", 100, "test", DateTime.UtcNow, new []
            {
                new AssetWeight("BTC", .1m, 100, false),
                new AssetWeight("LTC", .5m, 100, false)
            });
            
            // act

            bool result = index.ValidateWeights();

            // assert

            Assert.IsFalse(result, "The index cannot be valid if the sum of assets weights are less than 0.9");
        }
        
        [TestMethod]
        public void Index_Is_Not_Valid_If_Sum_Of_Assets_Weights_Are_Greater_Than_1_1()
        {
            // arrange
            
            var index = new Index("test", 100, "test", DateTime.UtcNow, new []
            {
                new AssetWeight("BTC", .7m, 100, false),
                new AssetWeight("LTC", .5m, 100, false)
            });
            
            // act

            bool result = index.ValidateWeights();

            // assert

            Assert.IsFalse(result, "The index cannot be valid if the sum of assets weights are greater than 1.1");
        }
        
        [TestMethod]
        public void Index_Is_Valid_If_Sum_Of_Assets_Weights_Between_0_9_And_1_1()
        {
            // arrange
            
            var index = new Index("test", 100, "test", DateTime.UtcNow, new []
            {
                new AssetWeight("BTC", .7m, 100, false),
                new AssetWeight("LTC", .3m, 100, false)
            });
            
            // act

            bool result = index.ValidateWeights();

            // assert

            Assert.IsTrue(result, "The index should be valid if the sum of assets weights are between 0.9 and 1.1");
        }
        
        [TestMethod]
        public void Index_Is_Not_Valid_If_Value_Less_Than_Zero()
        {
            // arrange
            
            var index = new Index("test", -100, "test", DateTime.UtcNow, new []
            {
                new AssetWeight("BTC", .7m, 100, false),
                new AssetWeight("LTC", .3m, 100, false)
            });
            
            // act

            bool result = index.ValidateValue();

            // assert

            Assert.IsFalse(result, "The index cannot be valid if the value is less than zero");
        }
        
        [TestMethod]
        public void Index_Is_Not_Valid_If_Value_Equal_To_Zero()
        {
            // arrange
            
            var index = new Index("test", 0, "test", DateTime.UtcNow, new []
            {
                new AssetWeight("BTC", .7m, 100, false),
                new AssetWeight("LTC", .3m, 100, false)
            });
            
            // act

            bool result = index.ValidateValue();

            // assert

            Assert.IsFalse(result, "The index cannot be valid if the value is equal to zero");
        }
        
        [TestMethod]
        public void Index_Is_Valid_If_Value_Greater_Than_Zero()
        {
            // arrange
            
            var index = new Index("test", 100, "test", DateTime.UtcNow, new []
            {
                new AssetWeight("BTC", .7m, 100, false),
                new AssetWeight("LTC", .3m, 100, false)
            });
            
            // act

            bool result = index.ValidateValue();

            // assert

            Assert.IsTrue(result, "The index should be valid if the value is greater than zero");
        }
    }
}
