using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.AssetPairs;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class AssetPairsController : Controller, IAssetPairsApi
    {
        private readonly IInstrumentService _instrumentService;

        public AssetPairsController(IInstrumentService instrumentService)
        {
            _instrumentService = instrumentService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of asset pairs.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<AssetPairSettingsModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<AssetPairSettingsModel>> GetAllAsync()
        {
            IReadOnlyCollection<AssetPairSettings> assetPairSettings = await _instrumentService.GetAssetPairsAsync();

            return Mapper.Map<AssetPairSettingsModel[]>(assetPairSettings);
        }

        /// <inheritdoc/>
        /// <response code="204">The asset pair settings successfully added.</response>
        /// <response code="400">An error occurred while adding asset pair settings.</response>
        /// <response code="409">The asset pair settings already exists.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        public async Task AddAsync([FromBody] AssetPairSettingsModel model, string userId)
        {
            try
            {
                var assetPairSettings = Mapper.Map<AssetPairSettings>(model);

                await _instrumentService.AddAssetPairAsync(assetPairSettings, userId);
            }
            catch (EntityAlreadyExistsException)
            {
                throw new ValidationApiException(HttpStatusCode.Conflict, "The asset pair settings already exists");
            }
            catch (InvalidOperationException exception)
            {
                throw new ValidationApiException(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset pair settings successfully updated.</response>
        /// <response code="400">An error occurred while updating asset pair settings.</response>
        /// <response code="404">The asset pair settings does not exist.</response>
        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task UpdateAsync([FromBody] AssetPairSettingsModel model, string userId)
        {
            try
            {
                var assetPairSettings = Mapper.Map<AssetPairSettings>(model);

                await _instrumentService.UpdateAssetPairAsync(assetPairSettings, userId);
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
        /// <response code="204">The asset pair settings successfully deleted.</response>
        /// <response code="400">An error occurred while deleting asset pair settings.</response>
        /// <response code="404">The asset pair settings does not exist.</response>
        [HttpDelete]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task DeleteAsync(string assetPair, string exchange, string userId)
        {
            try
            {
                await _instrumentService.DeleteAssetPairAsync(assetPair, exchange, userId);
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
    }
}
