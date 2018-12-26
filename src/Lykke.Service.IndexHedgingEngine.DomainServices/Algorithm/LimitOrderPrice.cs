using Lykke.Service.IndexHedgingEngine.Domain;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm
{
    public struct LimitOrderPrice
    {
        public LimitOrderPrice(decimal price, PriceType type)
        {
            Price = price;
            Type = type;
        }

        public decimal Price { get; }

        public PriceType Type { get; }
    }
}
