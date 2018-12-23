using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.MarketMaker;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Handlers;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class MarketMakerController : Controller, IMarketMakerApi
    {
        private readonly IMarketMakerStateService _marketMakerStateService;
        private readonly IMarketMakerStateHandler _marketMakerStateHandler;

        public MarketMakerController(
            IMarketMakerStateService marketMakerStateService,
            IMarketMakerStateHandler marketMakerStateHandler)
        {
            _marketMakerStateService = marketMakerStateService;
            _marketMakerStateHandler = marketMakerStateHandler;
        }

        /// <inheritdoc/>
        /// <response code="200">The market maker state.</response>
        [HttpGet("state")]
        [ProducesResponseType(typeof(MarketMakerStateModel), (int) HttpStatusCode.OK)]
        public async Task<MarketMakerStateModel> GetStateAsync()
        {
            MarketMakerState marketMakerState = await _marketMakerStateService.GetAsync();

            return Mapper.Map<MarketMakerStateModel>(marketMakerState);
        }

        /// <inheritdoc/>
        /// <response code="204">The market maker state successfully updated.</response>
        /// <response code="400">An error occurred while updating market maker state.</response>
        [HttpPost("state")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task SetStateAsync([FromBody] MarketMakerStateUpdateModel model)
        {
            try
            {
                await _marketMakerStateHandler.HandleMarketMakerStateAsync((Domain.MarketMakerStatus) model.Status,
                    model.Comment, model.UserId);
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(exception.Message);
            }
        }
    }
}
