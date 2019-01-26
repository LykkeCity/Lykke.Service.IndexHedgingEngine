using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Lykke.Service.IndexHedgingEngine.Domain.Simulation
{
    /// <summary>
    /// Represent the simulation parameters.
    /// </summary>
    public class SimulationParameters
    {
        public SimulationParameters()
        {
            Assets = new string[0];
        }

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// The amount of tokens that sold to the clients.
        /// </summary>
        public decimal OpenTokens { get; set; }

        /// <summary>
        /// The amount of USD that received from clients.
        /// </summary>
        public decimal Investments { get; set; }

        /// <summary>
        /// The collection of hedging assets. 
        /// </summary>
        public IReadOnlyCollection<string> Assets { get; set; }

        public void AddAsset(string asset)
        {
            Assets = Assets.Union(new[] {asset}).ToArray();
        }

        public void RemoveAsset(string asset)
        {
            Assets = Assets.Except(new[] {asset}).ToArray();
        }

        public void Update(decimal openTokens, decimal investments)
        {
            OpenTokens = openTokens;
            Investments = investments;
        }

        public static SimulationParameters Create(string indexName)
            => new SimulationParameters {IndexName = indexName};
    }
}
