using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settings;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class CrossAssetPairsController : Controller, ICrossAssetPairsApi
    {
        private readonly ICrossAssetPairSettingsService _crossAssetPairSettingsService;

        public CrossAssetPairsController(ICrossAssetPairSettingsService crossAssetPairSettingsService)
        {
            _crossAssetPairSettingsService = crossAssetPairSettingsService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of cross asset pairs.</response>
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
                var assetPairSettings = Mapper.Map<CrossAssetPairSettings>(model);

                await _crossAssetPairSettingsService.AddAsync(assetPairSettings, userId);
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

                await _crossAssetPairSettingsService.UpdateAsync(crossAssetPairSettings, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The asset pair settings does not exist");
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
        public async Task DeleteAsync(string assetPairId, string crossAssetPairId, string userId)
        {
            try
            {
                await _crossAssetPairSettingsService.DeleteAsync(assetPairId, crossAssetPairId, userId);
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
