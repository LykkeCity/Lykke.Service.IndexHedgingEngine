using JetBrains.Annotations;

namespace Lykke.Service.IndexHedgingEngine.Client.Models.Simulation
{
    /// <summary>
    /// Represent the simulation parameters.
    /// </summary>
    [PublicAPI]
    public class SimulationParametersModel
    {
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
    }
}
