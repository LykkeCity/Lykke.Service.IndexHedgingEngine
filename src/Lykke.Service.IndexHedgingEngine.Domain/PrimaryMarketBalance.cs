namespace Lykke.Service.IndexHedgingEngine.Domain
{
    public class PrimaryMarketBalance
    {
        public string AssetId { set; get; }
        public decimal Balance { set; get; }
        public decimal Reserved { get; set; }
    }
}
