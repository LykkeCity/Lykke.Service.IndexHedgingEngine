using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.AssetLinks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class AssetLinksController : Controller, IAssetLinksApi
    {
        private readonly IAssetLinkService _assetLinkService;

        public AssetLinksController(IAssetLinkService assetLinkService)
        {
            _assetLinkService = assetLinkService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of asset links.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<AssetLinkModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<AssetLinkModel>> GetAllAsync()
        {
            IReadOnlyCollection<AssetLink> assetLinks = await _assetLinkService.GetAllAsync();

            return Mapper.Map<List<AssetLinkModel>>(assetLinks);
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of asset identifiers.</response>
        [HttpGet("missed")]
        [ProducesResponseType(typeof(IReadOnlyCollection<string>), (int) HttpStatusCode.OK)]
        public Task<IReadOnlyCollection<string>> GetMissedAsync()
        {
            return _assetLinkService.GetMissedAsync();
        }

        /// <inheritdoc/>
        /// <response code="204">The asset link successfully added.</response>
        /// <response code="409">The asset link already exists.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        public async Task AddAsync([FromBody] AssetLinkModel model)
        {
            try
            {
                AssetLink assetLink = Mapper.Map<AssetLink>(model);

                await _assetLinkService.AddAsync(assetLink);
            }
            catch (EntityAlreadyExistsException)
            {
                throw new ValidationApiException(HttpStatusCode.Conflict, "The asset link already exists");
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset link successfully updated.</response>
        /// <response code="404">The asset link does not exist.</response>
        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task UpdateAsync([FromBody] AssetLinkModel model)
        {
            try
            {
                AssetLink assetLink = Mapper.Map<AssetLink>(model);

                await _assetLinkService.UpdateAsync(assetLink);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The asset link does not exist");
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The asset link successfully deleted.</response>
        /// <response code="404">The asset link does not exist.</response>
        [HttpDelete("{assetId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task DeleteAsync(string assetId)
        {
            try
            {
                await _assetLinkService.DeleteAsync(assetId);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The asset link does not exist");
            }
        }
    }
}
