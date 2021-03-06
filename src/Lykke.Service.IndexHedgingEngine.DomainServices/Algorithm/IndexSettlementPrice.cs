namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public struct IndexSettlementPrice
    {
        public IndexSettlementPrice(decimal price, decimal k, decimal r, decimal delta)
        {
            Price = price;
            K = k;
            R = r;
            Delta = delta;
        }

        public decimal Price { get; }

        public decimal K { get; }

        public decimal R { get; }

        public decimal Delta { get; }
    }
}
