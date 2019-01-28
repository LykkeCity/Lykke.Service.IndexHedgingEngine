using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Simulation;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with simulation.
    /// </summary>
    [PublicAPI]
    public interface ISimulationApi
    {
        /// <summary>
        /// Returns a simulation report by index name.
        /// </summary>
        /// <returns>The model that describes the simulation report.</returns>
        [Get("/api/Simulation/reports")]
        Task<SimulationReportModel> GetReportAsync(string indexName);

        /// <summary>
        /// Updates simulation parameters.
        /// </summary>
        /// <param name="model">The model that describes the simulation parameters.</param>
        [Post("/api/Simulation/parameters")]
        Task UpdateAsync([Body] SimulationParametersModel model);

        /// <summary>
        /// Adds asset to the index simulation.
        /// </summary>
        /// <param name="indexName">The name of index.</param>
        /// <param name="asset">The name of asses.</param>
        [Post("/api/Simulation/assets")]
        Task AddAssetAsync(string indexName, string asset);

        /// <summary>
        /// Removes asset to the index simulation.
        /// </summary>
        /// <param name="indexName">The name of index.</param>
        /// <param name="asset">The name of asses.</param>
        [Delete("/api/Simulation/assets")]
        Task RemoveAssetAsync(string indexName, string asset);
    }
}
