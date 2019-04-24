using System.Collections.Generic;

namespace Lykke.Service.IndexHedgingEngine.Domain.Reports
{
    public class InstrumentDeltaReport
    {
        public string Name { get; set; }        
        
        public string AssetId { get; set; }

        public decimal? Price { get; set; }

        public decimal Volume { get; set; }

        public decimal? VolumeInUsd { get; set; }
        
        public bool IsVirtual { get; set; }

        public IReadOnlyCollection<AssetDelta> AssetsDelta { get; set; }
    }
}
