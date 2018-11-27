using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.IndexPrices
{
    /// <summary>
    /// Represents an index state.
    /// </summary>
    [PublicAPI]
    public class IndexPriceModel
    {
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
        public IReadOnlyCollection<AssetWeightModel> Weights { get; set; }
    }
}
