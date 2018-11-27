using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.IndexPrices;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class IndexPricesController : Controller, IIndexPricesApi
    {
        private readonly IIndexPriceService _indexPriceService;

        public IndexPricesController(IIndexPriceService indexPriceService)
        {
            _indexPriceService = indexPriceService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of index states.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<IndexPriceModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<IndexPriceModel>> GetAllAsync()
        {
            IReadOnlyCollection<IndexPrice> indexStates = await _indexPriceService.GetAllAsync();

            return Mapper.Map<IndexPriceModel[]>(indexStates);
        }

        /// <inheritdoc/>
        /// <response code="200">The model that describes index state.</response>
        [HttpGet("{indexName}")]
        [ProducesResponseType(typeof(IndexPriceModel), (int) HttpStatusCode.OK)]
        public async Task<IndexPriceModel> GetByIndexAsync(string indexName)
        {
            IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(indexName);

            return Mapper.Map<IndexPriceModel>(indexPrice);
        }
    }
}
