using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.HedgeLimitOrders;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class HedgeLimitOrdersController : Controller, IHedgeLimitOrdersApi
    {
        private readonly IHedgeLimitOrderService _hedgeLimitOrderService;
        private readonly IHedgeService _hedgeService;

        public HedgeLimitOrdersController(
            IHedgeLimitOrderService hedgeLimitOrderService,
            IHedgeService hedgeService)
        {
            _hedgeLimitOrderService = hedgeLimitOrderService;
            _hedgeService = hedgeService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of hedge limit orders.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<HedgeLimitOrderModel>), (int) HttpStatusCode.OK)]
        public Task<IReadOnlyCollection<HedgeLimitOrderModel>> GetAllAsync()
        {
            IReadOnlyCollection<HedgeLimitOrder> hedgeLimitOrders = _hedgeLimitOrderService.GetAll();

            var model = Mapper.Map<HedgeLimitOrderModel[]>(hedgeLimitOrders);

            return Task.FromResult<IReadOnlyCollection<HedgeLimitOrderModel>>(model);
        }

        /// <inheritdoc/>
        /// <response code="204">Hedge limit order successfully created.</response>
        [HttpPost("create")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task CreateAsync([FromBody] HedgeLimitOrderCreateModel model, string userId)
        {
            try
            {
                await _hedgeService.CreateLimitOrderAsync(model.AssetId, model.Exchange, (LimitOrderType) model.Type,
                    model.Price, model.Volume, userId);
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">Hedge limit order successfully cancelled.</response>
        [HttpPost("cancel")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task CancelAsync([FromBody] HedgeLimitOrderCancelModel model, string userId)
        {
            try
            {
                await _hedgeService.CancelLimitOrderAsync(model.AssetId, model.Exchange, userId);
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }
    }
}
