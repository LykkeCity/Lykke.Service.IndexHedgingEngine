using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.IndexSettings;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class IndexSettingsController : Controller, IIndexSettingsApi
    {
        private readonly IIndexSettingsService _indexSettingsService;

        public IndexSettingsController(IIndexSettingsService indexSettingsService)
        {
            _indexSettingsService = indexSettingsService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of index settings.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<IndexSettingsModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<IndexSettingsModel>> GetAllAsync()
        {
            IReadOnlyCollection<IndexSettings> indexSettings = await _indexSettingsService.GetAllAsync();

            return Mapper.Map<List<IndexSettingsModel>>(indexSettings);
        }

        /// <inheritdoc/>
        /// <response code="200">The model that describes index settings.</response>
        /// <response code="404">The index settings does not exist.</response>
        [HttpGet("{indexName}")]
        [ProducesResponseType(typeof(IndexSettingsModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IndexSettingsModel> GetByIndexAsync(string indexName)
        {
            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(indexName);

            if (indexSettings == null)
                throw new ValidationApiException(HttpStatusCode.NotFound, "The index settings does not exist");

            return Mapper.Map<IndexSettingsModel>(indexSettings);
        }

        /// <inheritdoc/>
        /// <response code="204">The index settings successfully added.</response>
        /// <response code="409">The index settings already exists.</response>
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.Conflict)]
        public async Task AddAsync([FromBody] IndexSettingsModel model)
        {
            try
            {
                IndexSettings indexSettings = Mapper.Map<IndexSettings>(model);

                await _indexSettingsService.AddAsync(indexSettings);
            }
            catch (EntityAlreadyExistsException)
            {
                throw new ValidationApiException(HttpStatusCode.Conflict, "The index settings already exists");
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The index settings successfully updated.</response>
        /// <response code="404">The index settings does not exist.</response>
        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task UpdateAsync([FromBody] IndexSettingsModel model)
        {
            try
            {
                IndexSettings indexSettings = Mapper.Map<IndexSettings>(model);

                await _indexSettingsService.UpdateAsync(indexSettings);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The index settings does not exist");
            }
        }

        /// <inheritdoc/>
        /// <response code="204">The index settings successfully deleted.</response>
        /// <response code="404">The index settings does not exist.</response>
        [HttpDelete("{indexName}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task DeleteAsync(string indexName)
        {
            try
            {
                await _indexSettingsService.DeleteAsync(indexName);
            }
            catch (EntityNotFoundException)
            {
                throw new ValidationApiException(HttpStatusCode.NotFound, "The index settings does not exist");
            }
        }
    }
}
