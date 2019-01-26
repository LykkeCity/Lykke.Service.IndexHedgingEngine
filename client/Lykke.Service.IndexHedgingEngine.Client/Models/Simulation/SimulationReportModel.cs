using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Simulation
{
    /// <summary>
    /// Represents a result of index simulation results.
    /// </summary>
    [PublicAPI]
    public class SimulationReportModel
    {
        /// <summary>
        /// The name of the index.
        /// </summary>
        public string IndexName { get; set; }

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
        public decimal OpenTokens { get; set; }

        /// <summary>
        /// The amount of USD that received from clients.
        /// </summary>
        public decimal Investments { get; set; }

        /// <summary>
        /// The amount of open tokens in USD.
        /// </summary>
        public decimal AmountInUsd { get; set; }

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
        /// The profit and loss of current amount in USD and investments.
        /// </summary>
        public decimal PnL { get; set; }

        /// <summary>
        /// A collection of assets distributions.
        /// </summary>
        public IReadOnlyCollection<AssetDistributionModel> Assets { get; set; }
    }
}
