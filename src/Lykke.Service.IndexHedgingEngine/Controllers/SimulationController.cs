using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Simulation;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Simulation;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class SimulationController : Controller, ISimulationApi
    {
        private readonly ISimulationService _simulationService;

        public SimulationController(ISimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of settlements.</response>
        /// <response code="404">The simulation parameters not found.</response>
        [HttpGet("reports")]
        [ProducesResponseType(typeof(SimulationReportModel), (int) HttpStatusCode.OK)]
        public async Task<SimulationReportModel> GetReportAsync(string indexName)
        {
            SimulationReport simulationReport = await _simulationService.GetReportAsync(indexName);

            return Mapper.Map<SimulationReportModel>(simulationReport);
        }

        /// <inheritdoc/>
        /// <response code="204">The simulation parameters successfully approved.</response>
        /// <response code="400">An error occurred while updating simulation parameters.</response>
        [HttpPost("parameters")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task UpdateAsync([FromBody] SimulationParametersModel model)
        {
            await _simulationService.UpdateParametersAsync(model.IndexName, model.OpenTokens, model.Investments);
        }

        /// <inheritdoc/>
        /// <response code="204">The asset successfully added.</response>
        /// <response code="404">The simulation parameters not found.</response>
        [HttpPost("assets")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task AddAssetAsync(string indexName, string asset)
        {
            try
            {
                await _simulationService.AddAssetAsync(indexName, asset);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "Simulation parameters not found");
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset successfully removed.</response>
        /// <response code="404">The simulation parameters not found.</response>
        [HttpDelete("assets")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task RemoveAssetAsync(string indexName, string asset)
        {
            try
            {
                await _simulationService.RemoveAssetAsync(indexName, asset);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "Simulation parameters not found");
            }
        }
    }
}
