using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Positions;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class PositionsController : Controller, IPositionsApi
    {
        private readonly IPositionService _positionService;
        private readonly IHedgeService _hedgeService;

        public PositionsController(
            IPositionService positionService,
            IHedgeService hedgeService)
        {
            _positionService = positionService;
            _hedgeService = hedgeService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of positions.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<PositionModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<PositionModel>> GetAllAsync()
        {
            IReadOnlyCollection<Position> positionReports = await _positionService.GetAllAsync();

            return Mapper.Map<PositionModel[]>(positionReports);
        }

        /// <inheritdoc/>
        /// <response code="204">The position successfully closed.</response>
        /// <response code="400">An error occurred while closing position.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task CloseAsync([FromBody] ClosePositionOperationModel model, string userId)
        {
            try
            {
                await _hedgeService.ClosePositionAsync(model.AssetId, model.Exchange, userId);
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(exception.Message);
            }
        }
    }
}
