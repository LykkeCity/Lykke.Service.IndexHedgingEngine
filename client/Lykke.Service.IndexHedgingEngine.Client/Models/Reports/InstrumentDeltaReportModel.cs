using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Reports
{
    /// <summary>
    /// Represents a position delta report for an instrument.
    /// </summary>
    [PublicAPI]
    public class InstrumentDeltaReportModel
    {
        /// <summary>
        /// The name of the instrument.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The asset identifier.
        /// </summary>
        public string AssetId { get; set; }
        
        /// <summary>
        /// The price value.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// The volume value.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// The volume value in USD.
        /// </summary>
        public decimal? VolumeInUsd { get; set; }
        
        /// <summary>
        /// Indicates that the position is place on the virtual exchange.
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// The delta values for each asset from the index.
        /// </summary>
        public IReadOnlyCollection<AssetDeltaModel> AssetsDelta { get; set; }
    }
}
