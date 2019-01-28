using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class QuotesController : Controller, IQuotesApi
    {
        private readonly IQuoteService _quoteService;

        public QuotesController(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of quotes.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyCollection<QuoteModel>), (int) HttpStatusCode.OK)]
        public Task<IReadOnlyCollection<QuoteModel>> GetAsync(string exchange)
        {
            IEnumerable<Quote> quotes = _quoteService.GetAll();

            if (!string.IsNullOrEmpty(exchange))
                quotes = quotes.Where(o => o.Source == exchange);

            return Task.FromResult<IReadOnlyCollection<QuoteModel>>(Mapper.Map<QuoteModel[]>(quotes));
        }
    }
}
