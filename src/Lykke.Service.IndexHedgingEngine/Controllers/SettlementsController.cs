using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settlements;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class SettlementsController : Controller, ISettlementsApi
    {
        private readonly ISettlementService _settlementService;

        public SettlementsController(ISettlementService settlementService)
        {
            _settlementService = settlementService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of settlements.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<SettlementModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<SettlementModel>> GetAsync(string clientId = null)
        {
            IReadOnlyCollection<Settlement> settlements;

            if (string.IsNullOrEmpty(clientId))
                settlements = await _settlementService.GetAllAsync();
            else
                settlements = await _settlementService.GetByClientIdAsync(clientId);

            return Mapper.Map<SettlementModel[]>(settlements);
        }

        /// <inheritdoc/>
        /// <response code="200">The model that describes a settlement.</response>
        [HttpGet("{settlementId}")]
        [ProducesResponseType(typeof(SettlementModel), (int) HttpStatusCode.OK)]
        public async Task<SettlementModel> GetByIdAsync(string settlementId)
        {
            Settlement settlement;

            try
            {
                settlement = await _settlementService.GetByIdAsync(settlementId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "Settlement not found");
            }

            return Mapper.Map<SettlementModel>(settlement);
        }

        /// <inheritdoc/>
        /// <response code="204">The settlement successfully created.</response>
        /// <response code="400">An error occurred while creating settlement.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task CreateAsync([FromBody] SettlementRequestModel model, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "User id required");

            try
            {
                await _settlementService.CreateAsync(model.IndexName, model.Amount, model.Comment, model.WalletId,
                    model.ClientId, userId, model.IsDirect);
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The settlement successfully approved.</response>
        /// <response code="400">An error occurred while approving settlement.</response>
        /// <response code="404">The settlement not found.</response>
        [HttpPost("{settlementId}/approve")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task ApproveAsync(string settlementId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "User id required");

            try
            {
                await _settlementService.ApproveAsync(settlementId, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "Settlement not found");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The settlement successfully rejected.</response>
        /// <response code="400">An error occurred while rejecting settlement.</response>
        /// <response code="404">The settlement not found.</response>
        [HttpPost("{settlementId}/reject")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task RejectAsync(string settlementId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "User id required");

            try
            {
                await _settlementService.RejectAsync(settlementId, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "Settlement not found");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The settlement successfully recalculated.</response>
        /// <response code="400">An error occurred while recalculating settlement.</response>
        /// <response code="404">The settlement not found.</response>
        [HttpPost("{settlementId}/recalculate")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task RecalculateAsync(string settlementId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "User id required");

            try
            {
                await _settlementService.RecalculateAsync(settlementId, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "Settlement not found");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The settlement successfully validated.</response>
        /// <response code="400">An error occurred while validating settlement.</response>
        /// <response code="404">The settlement not found.</response>
        [HttpPost("{settlementId}/validate")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task ValidateAsync(string settlementId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "User id required");

            try
            {
                await _settlementService.ValidateAsync(settlementId, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "Settlement not found");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset settlement successfully updated.</response>
        /// <response code="400">An error occurred while updating settlement.</response>
        /// <response code="404">The settlement not found.</response>
        [HttpPut("{settlementId}/assets")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task UpdateAssetAsync([FromBody] AssetSettlementEditModel model, string settlementId,
            string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "User id required");

            try
            {
                await _settlementService.UpdateAssetAsync(settlementId, model.AssetId, model.Amount, model.IsDirect,
                    model.IsExternal, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "Settlement not found");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset settlement successfully retried.</response>
        /// <response code="400">An error occurred while retrying settlement.</response>
        /// <response code="404">The settlement not found.</response>
        [HttpPost("{settlementId}/assets/{assetId}/retry")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task RetryAssetAsync(string settlementId, string assetId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "User id required");

            try
            {
                await _settlementService.RetryAssetAsync(settlementId, assetId, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "Settlement not found");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset settlement successfully executed.</response>
        /// <response code="400">An error occurred while executing settlement.</response>
        /// <response code="404">The settlement not found.</response>
        [HttpPost("{settlementId}/assets/{assetId}/execute")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task ExecuteAssetAsync(string settlementId, string assetId, decimal actualAmount,
            decimal actualPrice, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ValidationApiException(HttpStatusCode.BadRequest, "User id required");

            try
            {
                await _settlementService.ExecuteAssetAsync(settlementId, assetId, actualAmount, actualPrice, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "Settlement not found");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }
    }
}
