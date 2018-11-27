using System;
using System.Collections.Generic;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents an index state.
    /// </summary>
    public class IndexPrice
    {
        private const decimal InitialPrice = 100;
        private const decimal InitialK = 0;

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The current value of index.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// The current price of index. 
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The performance coefficient that was used to calculate <see cref="Price"/>.
        /// </summary>
        public decimal K { get; set; }

        /// <summary>
        /// The R coefficient that was used to calculate <see cref="Price"/>.
        /// </summary>
        public decimal R { get; set; }

        /// <summary>
        /// The delta of investment period that was used to calculate <see cref="Price"/>.
        /// </summary>
        public decimal Delta { get; set; }

        /// <summary>
        /// The date of index value.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// A collection of weights of assets in the current index.
        /// </summary>
        public IReadOnlyCollection<AssetWeight> Weights { get; set; }

        public void Update(decimal value, DateTime timestamp, decimal price, decimal k, decimal r, decimal delta,
            IReadOnlyCollection<AssetWeight> weights)
        {
            Value = value;
            Timestamp = timestamp;
            Price = price;
            K = k;
            R = r;
            Delta = delta;
            Weights = weights;
        }

        // TODO: refactoring
        public void Update(IndexPrice indexPrice)
        {
            Value = indexPrice.Value;
            Timestamp = indexPrice.Timestamp;
            Price = indexPrice.Price;
            K = indexPrice.K;
            R = indexPrice.R;
            Delta = indexPrice.Delta;
            Weights = indexPrice.Weights;
        }

        public static IndexPrice Init(string name, decimal value, DateTime timestamp,
            IReadOnlyCollection<AssetWeight> weights)
        {
            return new IndexPrice
            {
                Name = name,
                Value = value,
                Timestamp = timestamp,
                Price = InitialPrice,
                K = InitialK,
                R = decimal.Zero,
                Delta = decimal.Zero,
                Weights = weights
            };
        }
    }
}
