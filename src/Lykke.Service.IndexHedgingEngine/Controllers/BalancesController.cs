using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Balances;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class BalancesController : Controller, IBalancesApi
    {
        private readonly IBalanceService _balanceService;

        public BalancesController(IBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of balances.</response>
        [HttpGet("lykke")]
        [ProducesResponseType(typeof(IReadOnlyCollection<BalanceModel>), (int) HttpStatusCode.OK)]
        public Task<IReadOnlyList<BalanceModel>> GetLykkeAsync()
        {
            IReadOnlyCollection<Balance> balances = _balanceService.GetByExchange(ExchangeNames.Lykke);

            var model = Mapper.Map<BalanceModel[]>(balances);

            return Task.FromResult<IReadOnlyList<BalanceModel>>(model);
        }
    }
}
