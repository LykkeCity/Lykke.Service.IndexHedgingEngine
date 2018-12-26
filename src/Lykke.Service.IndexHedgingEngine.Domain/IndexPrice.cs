using System;
using System.Collections.Generic;
using System.Linq;

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
        /// The current settlement price of index. 
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

        public bool ValidateValue()
            => Value > 0;
        
        public bool ValidateWeights()
            => Math.Abs(Weights.Sum(o => o.Weight) - 1) < 0.1m;
        
        public void Update(decimal value, DateTime timestamp, decimal price, decimal k, decimal r, decimal delta,
            IEnumerable<AssetWeight> weights)
        {
            Value = value;
            Timestamp = timestamp;
            Price = price;
            K = k;
            R = r;
            Delta = delta;
            Weights = weights.ToArray();
        }

        public static IndexPrice Init(string name, decimal value, DateTime timestamp, IEnumerable<AssetWeight> weights)
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
                Weights = weights.ToArray()
            };
        }
    }
}
