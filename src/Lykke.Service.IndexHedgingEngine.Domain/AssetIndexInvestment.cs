namespace Lykke.Service.IndexHedgingEngine.Domain
{
    /// <summary>
    /// Represents an asset investment in index details.
    /// </summary>
    public class AssetIndexInvestment
    {
        /// <summary>
        /// The name of the index.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the index.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// The price of the index.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The open volume of the token.
        /// </summary>
        public decimal OpenVolume { get; set; }

        /// <summary>
        /// The opposite USD volume received from clients.
        /// </summary>
        public decimal OppositeVolume { get; set; }

        /// <summary>
        /// The weight of the asset in the index.
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// The asset amount of investment in index.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
