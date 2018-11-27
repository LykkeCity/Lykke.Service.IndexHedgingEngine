using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Tokens;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class TokensController : Controller, ITokensApi
    {
        private readonly ITokenService _tokenService;

        public TokensController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <inheritdoc/>
        /// <response code="200">>A collection of tokens amount.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<TokenModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<TokenModel>> GetAllAsync()
        {
            IReadOnlyCollection<Token> tokens = await _tokenService.GetAllAsync();

            return Mapper.Map<TokenModel[]>(tokens);
        }

        /// <inheritdoc/>
        /// <response code="204">A token operation successfully executed.</response>
        /// <response code="400">The token amount can not be less than zero.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task UpdateAsync([FromBody] TokenOperationModel model)
        {
            try
            {
                await _tokenService.UpdateAmountAsync(model.AssetId, (BalanceOperationType) model.Type, model.Amount,
                    model.Comment, model.UserId);
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }
    }
}
