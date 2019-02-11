using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Trades;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class TradesController : Controller, ITradesApi
    {
        private readonly IInternalTradeService _internalTradeService;
        private readonly IVirtualTradeService _virtualTradeService;
        private readonly ILykkeTradeService _lykkeTradeService;
        private readonly IExternalTradeService _externalTradeService;

        public TradesController(
            IInternalTradeService internalTradeService,
            IVirtualTradeService virtualTradeService,
            ILykkeTradeService lykkeTradeService,
            IExternalTradeService externalTradeService)
        {
            _internalTradeService = internalTradeService;
            _virtualTradeService = virtualTradeService;
            _lykkeTradeService = lykkeTradeService;
            _externalTradeService = externalTradeService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of internal trades.</response>
        [HttpGet("internal")]
        [ProducesResponseType(typeof(IReadOnlyCollection<InternalTradeModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<InternalTradeModel>> GetInternalTradesAsync(DateTime startDate,
            DateTime endDate, string assetPairId, string oppositeWalletId, int limit)
        {
            IReadOnlyCollection<InternalTrade> internalTrades =
                await _internalTradeService.GetAsync(startDate, endDate, assetPairId, oppositeWalletId, limit);

            return Mapper.Map<InternalTradeModel[]>(internalTrades);
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of virtual trades.</response>
        [HttpGet("virtual")]
        [ProducesResponseType(typeof(IReadOnlyCollection<InternalTradeModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<VirtualTradeModel>> GetVirtualTradesAsync(DateTime startDate,
            DateTime endDate, string assetPairId, int limit)
        {
            IReadOnlyCollection<VirtualTrade> virtualTrades =
                await _virtualTradeService.GetAsync(startDate, endDate, assetPairId, limit);

            return Mapper.Map<VirtualTradeModel[]>(virtualTrades);
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of Lykke trades.</response>
        [HttpGet("lykke")]
        [ProducesResponseType(typeof(IReadOnlyCollection<InternalTradeModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<InternalTradeModel>> GetLykkeTradesAsync(DateTime startDate,
            DateTime endDate, string assetPairId, string oppositeWalletId, int limit)
        {
            IReadOnlyCollection<InternalTrade> internalTrades =
                await _lykkeTradeService.GetAsync(startDate, endDate, assetPairId, oppositeWalletId, limit);

            return Mapper.Map<InternalTradeModel[]>(internalTrades);
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of external trades.</response>
        [HttpGet("external")]
        [ProducesResponseType(typeof(IReadOnlyCollection<ExternalTradeModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<ExternalTradeModel>> GetExternalTradesAsync(DateTime startDate,
            DateTime endDate, string exchange, string assetPairId, int limit)
        {
            IReadOnlyCollection<ExternalTrade> externalTrades =
                await _externalTradeService.GetAsync(startDate, endDate, exchange, assetPairId, limit);

            return Mapper.Map<ExternalTradeModel[]>(externalTrades);
        }

        /// <inheritdoc/>
        /// <response code="200">An internal trade.</response>
        /// <response code="404">Internal trade does not exist.</response>
        [HttpGet("internal/{tradeId}")]
        [ProducesResponseType(typeof(InternalTradeModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<InternalTradeModel> GetInternalTradeByIdAsync(string tradeId)
        {
            InternalTrade internalTrade = await _internalTradeService.GetByIdAsync(tradeId);

            if (internalTrade == null)
                throw new ValidationApiException(HttpStatusCode.NotFound, "Internal trade does not exist.");

            return Mapper.Map<InternalTradeModel>(internalTrade);
        }
    }
}
