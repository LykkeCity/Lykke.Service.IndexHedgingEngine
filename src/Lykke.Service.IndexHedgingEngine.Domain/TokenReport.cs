using System.Collections.Generic;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    public class TokenReport
    {
        public string Name { get; set; }

        public string AssetId { get; set; }

        public decimal? Price { get; set; }
		
        public decimal OpenVolume { get; set; }
		
        public IReadOnlyCollection<AssetWeight> Weights { get; set; }
    }
}
