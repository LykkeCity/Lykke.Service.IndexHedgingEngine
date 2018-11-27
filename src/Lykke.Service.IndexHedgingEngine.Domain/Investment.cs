namespace Lykke.Service.IndexHedgingEngine.Domain
{
    public class Investment
    {
        public string AssetId { get; set; }

        public string Exchange { get; set; }

        public decimal Volume { get; set; }

        public decimal RemainingVolume { get; set; }

        public string Error { get; set; }
    }
}
