using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Funding;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class FundingController : Controller, IFundingApi
    {
        private readonly IFundingService _fundingService;

        public FundingController(IFundingService fundingService)
        {
            _fundingService = fundingService;
        }

        /// <inheritdoc/>
        /// <response code="200">The model that represents funding amount.</response>
        [HttpGet]
        [ProducesResponseType(typeof(FundingModel), (int) HttpStatusCode.OK)]
        public async Task<FundingModel> GetAllAsync()
        {
            Funding funding = await _fundingService.GetAsync();

            return Mapper.Map<FundingModel>(funding);
        }

        /// <inheritdoc/>
        /// <response code="204">A funding operation successfully executed.</response>
        /// <response code="400">Funding amount can not be less than zero.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task UpdateAsync([FromBody] FundingOperationModel model)
        {
            try
            {
                await _fundingService.UpdateAsync((BalanceOperationType) model.Type, model.Amount, model.Comment,
                    model.UserId);
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }
    }
}
