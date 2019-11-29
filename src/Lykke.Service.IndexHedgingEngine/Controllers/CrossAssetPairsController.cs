using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settings;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Handlers;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using CrossAssetPairSettingsMode = Lykke.Service.IndexHedgingEngine.Client.Models.Settings.CrossAssetPairSettingsMode;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class CrossAssetPairsController : Controller, ICrossAssetPairsApi
    {
        private readonly ICrossAssetPairSettingsService _crossAssetPairSettingsService;
        private readonly IMarketMakerStateHandler _marketMakerStateHandler;

        public CrossAssetPairsController(ICrossAssetPairSettingsService crossAssetPairSettingsService,
            IMarketMakerStateHandler marketMakerStateHandler)
        {
            _crossAssetPairSettingsService = crossAssetPairSettingsService;
            _marketMakerStateHandler = marketMakerStateHandler;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of asset pairs.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<CrossAssetPairSettingsModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<CrossAssetPairSettingsModel>> GetAllAsync()
        {
            IReadOnlyCollection<CrossAssetPairSettings> crossAssetPairSettings = await _crossAssetPairSettingsService.GetAllAsync();

            return Mapper.Map<CrossAssetPairSettingsModel[]>(crossAssetPairSettings);
        }

        /// <inheritdoc/>
        /// <response code="204">The cross asset pair settings successfully added.</response>
        /// <response code="400">An error occurred while adding cross asset pair settings.</response>
        /// <response code="409">The cross asset pair settings already exists.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        public async Task AddAsync([FromBody] CrossAssetPairSettingsModel model, string userId)
        {
            try
            {
                var crossAssetPairSettings = Mapper.Map<CrossAssetPairSettings>(model);

                await _crossAssetPairSettingsService.AddCrossAssetPairAsync(crossAssetPairSettings, userId);
            }
            catch (EntityAlreadyExistsException)
            {
                throw new ValidationApiException(HttpStatusCode.Conflict, "The cross asset pair settings already exists");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The cross asset pair settings successfully updated.</response>
        /// <response code="400">An error occurred while updating cross asset pair settings.</response>
        /// <response code="404">The cross asset pair settings does not exist.</response>
        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task UpdateAsync([FromBody] CrossAssetPairSettingsModel model, string userId)
        {
            try
            {
                var crossAssetPairSettings = Mapper.Map<CrossAssetPairSettings>(model);

                await _crossAssetPairSettingsService.UpdateCrossAssetPairAsync(crossAssetPairSettings, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The cross asset pair settings does not exist");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The cross asset pair settings successfully updated.</response>
        /// <response code="400">An error occurred while updating cross asset pair settings.</response>
        /// <response code="404">The cross asset pair settings does not exist.</response>
        [HttpPut("updateMode")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        public async Task UpdateModeAsync(Guid id, CrossAssetPairSettingsMode mode, string userId)
        {
            try
            {
                await _marketMakerStateHandler.HandleCrossAssetPairStateAsync(id, Mapper.Map<Domain.CrossAssetPairSettingsMode>(mode), userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The cross asset pair settings does not exist");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The cross asset pair settings successfully deleted.</response>
        /// <response code="400">An error occurred while deleting cross asset pair settings.</response>
        /// <response code="404">The cross asset pair settings does not exist.</response>
        [HttpDelete]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task DeleteAsync(Guid id, string userId)
        {
            try
            {
                await _crossAssetPairSettingsService.DeleteCrossAssetPairAsync(id, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The cross asset pair settings does not exist");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }
    }
}
