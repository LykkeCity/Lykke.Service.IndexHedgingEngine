using System;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    public class PrimaryMarketHistoryItem
    {
        public DateTime DateTime { set; get; }
        public string AssetId { set; get; }
        public decimal Amount { set; get; }
        public string UserId { set; get; }
        public string Comment { set; get; }
    }
}
