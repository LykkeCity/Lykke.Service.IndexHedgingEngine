using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.OrderBooks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class OrderBooksController : Controller, IOrderBooksApi
    {
        private readonly IOrderBookService _orderBookService;
        private readonly ILimitOrderService _limitOrderService;

        public OrderBooksController(
            IOrderBookService orderBookService,
            ILimitOrderService limitOrderService)
        {
            _orderBookService = orderBookService;
            _limitOrderService = limitOrderService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of order books.</response>
        [Obsolete]
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<OrderBookModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<OrderBookModel>> GetAllAsync(int limit)
        {
            return await GetLykkeAsync(limit);
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of order books.</response>
        [HttpGet("lykke")]
        [ProducesResponseType(typeof(IReadOnlyCollection<OrderBookModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<OrderBookModel>> GetLykkeAsync(int limit)
        {
            limit = limit <= 0 ? int.MaxValue : limit;

            IReadOnlyCollection<OrderBook> orderBooks = await _orderBookService.GetAsync(limit);

            return Mapper.Map<OrderBookModel[]>(orderBooks);
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of order books.</response>
        [HttpGet("internal")]
        [ProducesResponseType(typeof(IReadOnlyCollection<OrderBookModel>), (int) HttpStatusCode.OK)]
        public Task<IReadOnlyCollection<OrderBookModel>> GetInternalAsync()
        {
            IReadOnlyCollection<OrderBook> orderBooks = _limitOrderService.GetAll();

            return Task.FromResult<IReadOnlyCollection<OrderBookModel>>(Mapper.Map<OrderBookModel[]>(orderBooks));
        }
    }
}
