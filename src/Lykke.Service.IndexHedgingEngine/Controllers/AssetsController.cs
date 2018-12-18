using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Assets;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class AssetsController : Controller, IAssetsApi
    {
        private readonly IInstrumentService _instrumentService;

        public AssetsController(IInstrumentService instrumentService)
        {
            _instrumentService = instrumentService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of assets.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<AssetSettingsModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<AssetSettingsModel>> GetAllAsync()
        {
            IReadOnlyCollection<AssetSettings> assetSettings = await _instrumentService.GetAssetsAsync();

            return Mapper.Map<AssetSettingsModel[]>(assetSettings);
        }

        /// <inheritdoc/>
        /// <response code="204">The asset settings successfully added.</response>
        /// <response code="409">The asset settings already exists.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        public async Task AddAsync([FromBody] AssetSettingsModel model, string userId)
        {
            try
            {
                var assetSettings = Mapper.Map<AssetSettings>(model);

                await _instrumentService.AddAssetAsync(assetSettings, userId);
            }
            catch (EntityAlreadyExistsException)
            {
                throw new ValidationApiException(HttpStatusCode.Conflict, "The asset settings already exists");
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset settings successfully updated.</response>
        /// <response code="404">The asset settings does not exist.</response>
        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task UpdateAsync([FromBody] AssetSettingsModel model, string userId)
        {
            try
            {
                var assetSettings = Mapper.Map<AssetSettings>(model);

                await _instrumentService.UpdateAssetAsync(assetSettings, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The asset settings does not exist");
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset settings successfully deleted.</response>
        /// <response code="400">An error occurred while deleting asset settings.</response>
        /// <response code="404">The asset settings does not exist.</response>
        [HttpDelete]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task DeleteAsync(string asset, string exchange, string userId)
        {
            try
            {
                await _instrumentService.DeleteAssetAsync(asset, exchange, userId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The asset settings does not exist");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }
    }
}
