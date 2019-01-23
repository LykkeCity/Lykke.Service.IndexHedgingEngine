using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Balances;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class BalancesController : Controller, IBalancesApi
    {
        private readonly IBalanceService _balanceService;

        public BalancesController(IBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of balances.</response>
        [HttpGet("lykke")]
        [ProducesResponseType(typeof(IReadOnlyCollection<BalanceModel>), (int) HttpStatusCode.OK)]
        public Task<IReadOnlyList<BalanceModel>> GetLykkeAsync()
        {
            IReadOnlyCollection<Balance> balances = _balanceService.GetByExchange(ExchangeNames.Lykke);

            var model = Mapper.Map<BalanceModel[]>(balances);

            return Task.FromResult<IReadOnlyList<BalanceModel>>(model);
        }

        /// <inheritdoc/>
        /// <response code="204">A balance operation successfully executed.</response>
        /// <response code="400">Balance can not be less than zero.</response>
        [HttpPost("lykke")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task UpdateAsync([FromBody] AssetBalanceOperationModel model, string userId)
        {
            try
            {
                await _balanceService.UpdateAsync(model.AssetId, (BalanceOperationType) model.Type, model.Amount,
                    model.Comment, userId);
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }
    }
}
