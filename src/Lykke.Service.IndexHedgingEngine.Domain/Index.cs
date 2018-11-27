using System;
using System.Collections.Generic;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents an index value on time.
    /// </summary>
    public class Index
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Index"/> with parameters.
        /// </summary>
        /// <param name="name">The name of the index.</param>
        /// <param name="value">The current value of index.</param>
        /// <param name="source">The name of the index source.</param>
        /// <param name="timestamp">The date of index value.</param>
        /// <param name="weights">A collection of weights of assets in the current index.</param>
        public Index(string name, decimal value, string source, DateTime timestamp,
            IReadOnlyCollection<AssetWeight> weights)
        {
            Source = source;
            Name = name;
            Value = value;
            Timestamp = timestamp;
            Weights = weights;
        }

        /// <summary>
        /// The name of the index (LCI, P-LCI or SC-LCI).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The current value of index.
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// The name of the index source (always equals to "LCI").
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// The date of index value.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// A collection of weights of assets in the current index.
        /// </summary>
        public IReadOnlyCollection<AssetWeight> Weights { get; }
    }
}
