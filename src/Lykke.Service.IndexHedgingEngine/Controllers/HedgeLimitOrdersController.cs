using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
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

        public HedgeLimitOrdersController(IHedgeLimitOrderService hedgeLimitOrderService)
        {
            _hedgeLimitOrderService = hedgeLimitOrderService;
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
    }
}
