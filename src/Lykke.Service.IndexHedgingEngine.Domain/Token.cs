using System;

namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents an amount of produced tokens.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// The identifier of the asset that associated with token.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The amount of produced tokens.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The amount of tokens that sold to the clients.
        /// </summary>
        public decimal OpenVolume { get; set; }
        
        /// <summary>
        /// The amount of USD that received from clients.
        /// </summary>
        public decimal OppositeVolume { get; set; }
        
        public void IncreaseAmount(decimal amount)
        {
            Amount += amount;
        }
        
        public void DecreaseAmount(decimal amount)
        {
            if (Amount - amount < 0)
                throw new InvalidOperationException("Amount can not be less than zero");

            Amount -= amount;
        }
        
        public void IncreaseVolume(decimal volume, decimal oppositeVolume)
        {
            OpenVolume += volume;
            OppositeVolume += oppositeVolume;
        }
        
        public void DecreaseVolume(decimal volume, decimal oppositeVolume)
        {
            if (OpenVolume - volume < 0)
                throw new InvalidOperationException("Open volume can not be less than zero");
            
            OpenVolume -= volume;
            OppositeVolume -= oppositeVolume;
        }

        public void Close(decimal volume, decimal price)
        {
            OpenVolume -= volume;
            OppositeVolume -= volume * price;
        }

        public Token Copy()
        {
            return new Token
            {
                AssetId = AssetId,
                Amount = Amount,
                OpenVolume = OpenVolume,
                OppositeVolume = OppositeVolume
            };
        }
    }
}
