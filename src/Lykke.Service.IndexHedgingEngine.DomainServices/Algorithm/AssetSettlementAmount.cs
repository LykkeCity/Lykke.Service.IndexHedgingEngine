namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public class AssetSettlementAmount
    {
        public AssetSettlementAmount(string assetId, decimal amount, decimal price, decimal weight)
        {
            AssetId = assetId;
            Amount = amount;
            Price = price;
            Weight = weight;
        }

        public string AssetId { get; }

        public decimal Amount { get; }

        public decimal Price { get; }

        public decimal Weight { get; }
    }
}
