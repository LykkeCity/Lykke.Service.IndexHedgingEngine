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
    public class CrossIndexController : Controller, ICrossIndexApi
    {
        private readonly ICrossIndexSettingsService _crossIndexSettingsService;

        public CrossIndexController(ICrossIndexSettingsService crossIndexSettingsService)
        {
            _crossIndexSettingsService = crossIndexSettingsService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of cross indices.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<CrossIndexSettingsModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<CrossIndexSettingsModel>> GetAllAsync()
        {
            IReadOnlyCollection<CrossIndexSettings> crossAssetPairSettings = await _crossIndexSettingsService.GetAllAsync();

            return Mapper.Map<CrossIndexSettingsModel[]>(crossAssetPairSettings);
        }

        /// <inheritdoc/>
        /// <response code="204">The cross index settings successfully added.</response>
        /// <response code="400">An error occurred while adding cross index settings.</response>
        /// <response code="409">The cross index settings already exists.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        public async Task<Guid> AddAsync([FromBody] CrossIndexSettingsModel model, string userId)
        {
            try
            {
                var assetPairSettings = Mapper.Map<CrossIndexSettings>(model);

                var id = await _crossIndexSettingsService.AddAsync(assetPairSettings, userId);

                return id;
            }
            catch (EntityAlreadyExistsException)
            {
                throw new ValidationApiException(HttpStatusCode.Conflict, "The cross index settings already exists");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
            catch (ArgumentException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The cross index settings successfully updated.</response>
        /// <response code="400">An error occurred while updating cross index settings.</response>
        /// <response code="404">The cross index settings does not exist.</response>
        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task UpdateAsync([FromBody] CrossIndexSettingsModel model, string userId)
        {
            try
            {
                var crossAssetPairSettings = Mapper.Map<CrossIndexSettings>(model);

                await _crossIndexSettingsService.UpdateAsync(crossAssetPairSettings, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The cross index settings does not exist");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
            catch (ArgumentException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The cross index settings successfully deleted.</response>
        /// <response code="400">An error occurred while deleting cross index settings.</response>
        /// <response code="404">The cross index settings does not exist.</response>
        [HttpDelete]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task DeleteAsync(Guid id, string userId)
        {
            try
            {
                await _crossIndexSettingsService.DeleteAsync(id, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The cross index settings does not exist");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
            catch (ArgumentException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }
    }
}
