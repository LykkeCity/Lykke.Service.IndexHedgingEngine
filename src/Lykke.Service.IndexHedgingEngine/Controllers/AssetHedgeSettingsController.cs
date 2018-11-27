using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.AssetHedgeSettings;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class AssetHedgeSettingsController : Controller, IAssetHedgeSettingsApi
    {
        private readonly IAssetHedgeSettingsService _assetHedgeSettingsService;

        public AssetHedgeSettingsController(IAssetHedgeSettingsService assetHedgeSettingsService)
        {
            _assetHedgeSettingsService = assetHedgeSettingsService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of asset hedge settings.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<AssetHedgeSettingsModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<AssetHedgeSettingsModel>> GetAllAsync()
        {
            IReadOnlyCollection<AssetHedgeSettings> assetHedgeSettings = await _assetHedgeSettingsService.GetAllAsync();

            return Mapper.Map<List<AssetHedgeSettingsModel>>(assetHedgeSettings);
        }

        /// <inheritdoc/>
        /// <response code="200">The model that describes asset hedge settings.</response>
        /// <response code="404">The asset hedge settings does not exist.</response>
        [HttpGet("{assetId}")]
        [ProducesResponseType(typeof(AssetHedgeSettingsModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<AssetHedgeSettingsModel> GetByIndexAsync(string assetId)
        {
            AssetHedgeSettings assetHedgeSettings = await _assetHedgeSettingsService.GetByAssetIdAsync(assetId);

            if (assetHedgeSettings == null)
                throw new ValidationApiException(HttpStatusCode.NotFound, "The asset hedge settings does not exist");

            return Mapper.Map<AssetHedgeSettingsModel>(assetHedgeSettings);
        }

        /// <inheritdoc/>
        /// <response code="204">The asset hedge settings successfully added.</response>
        /// <response code="409">The asset hedge settings already exists.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        public async Task AddAsync([FromBody] AssetHedgeSettingsEditModel model)
        {
            try
            {
                AssetHedgeSettings assetHedgeSettings = Mapper.Map<AssetHedgeSettings>(model);

                await _assetHedgeSettingsService.AddAsync(assetHedgeSettings);
            }
            catch (EntityAlreadyExistsException)
            {
                throw new ValidationApiException(HttpStatusCode.Conflict, "The asset hedge settings already exists");
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset hedge settings successfully updated.</response>
        /// <response code="404">The asset hedge settings does not exist.</response>
        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task UpdateAsync([FromBody] AssetHedgeSettingsEditModel model)
        {
            try
            {
                AssetHedgeSettings assetHedgeSettings = Mapper.Map<AssetHedgeSettings>(model);

                await _assetHedgeSettingsService.UpdateAsync(assetHedgeSettings);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The asset hedge settings does not exist");
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset hedge settings successfully deleted.</response>
        /// <response code="404">The asset hedge settings does not exist.</response>
        [HttpDelete("{assetId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task DeleteAsync(string assetId)
        {
            try
            {
                await _assetHedgeSettingsService.DeleteAsync(assetId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The asset hedge settings does not exist");
            }
        }
    }
}
