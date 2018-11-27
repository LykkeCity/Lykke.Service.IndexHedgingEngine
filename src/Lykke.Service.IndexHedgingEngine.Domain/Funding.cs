using System;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents a funding amount.
    /// </summary>
    public class Funding
    {
        /// <summary>
        /// The amount of funds.
        /// </summary>
        public decimal Amount { get; set; }
        
        public void Add(decimal amount)
        {
            Amount += amount;
        }
        
        public void Subtract(decimal amount)
        {
            if (Amount - amount < 0)
                throw new InvalidOperationException("Amount can not be less than zero");

            Amount -= amount;
        }

        public Funding Copy()
        {
            return new Funding {Amount = Amount};
        }
    }
}
