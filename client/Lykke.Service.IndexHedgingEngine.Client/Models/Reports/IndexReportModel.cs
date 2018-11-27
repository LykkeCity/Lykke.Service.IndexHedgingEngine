using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.IndexPrices;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Reports
{
    /// <summary>
    /// Represents an index report.
    /// </summary>
    [PublicAPI]
    public class IndexReportModel
    {
        /// <summary>
        /// The name of the index.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The current value of index.
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// The current price of index. 
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// The date of index value.
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// The performance coefficient.
        /// </summary>
        public decimal? K { get; set; }

        /// <summary>
        /// The amount of produced tokens.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The amount of tokens that sold to the clients.
        /// </summary>
        public decimal OpenVolume { get; set; }

        /// <summary>
        /// The amount of USD that received from clients.
        /// </summary>
        public decimal OppositeVolume { get; set; }

        /// <summary>
        /// The current balance.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// The identifier of the asset that associated with token.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The identifier of the asset pair that used to create limit orders.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The alpha coefficient.
        /// </summary>
        public decimal Alpha { get; set; }

        /// <summary>
        /// The tracking fee.
        /// </summary>
        public decimal TrackingFee { get; set; }

        /// <summary>
        /// The performance fee.
        /// </summary>
        public decimal PerformanceFee { get; set; }

        /// <summary>
        /// The sell limit order markup.
        /// </summary>
        public decimal SellMarkup { get; set; }

        /// <summary>
        /// The volume of sell limit order.
        /// </summary>
        public decimal SellVolume { get; set; }

        /// <summary>
        /// The volume of buy limit order.
        /// </summary>
        public decimal BuyVolume { get; set; }

        /// <summary>
        /// A collection of weights of assets in the current index.
        /// </summary>
        public IReadOnlyCollection<AssetWeightModel> Weights { get; set; }
    }
}
