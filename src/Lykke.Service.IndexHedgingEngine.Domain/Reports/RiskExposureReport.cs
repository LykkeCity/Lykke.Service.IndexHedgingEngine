using System.Collections.Generic;

namespace Lykke.Service.IndexHedgingEngine.Domain.Reports
{
    public class RiskExposureReport
    {
        public decimal UsdCash { get; set; }
        
        public IReadOnlyCollection<TokenReport> Indices { get; set; }
        
        public IReadOnlyCollection<InstrumentDeltaReport> Tokens { get; set; }
	
        public IReadOnlyCollection<InstrumentDeltaReport> Assets { get; set; }
    }
}
