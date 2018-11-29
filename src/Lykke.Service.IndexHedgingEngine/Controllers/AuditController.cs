using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models;
using Lykke.Service.IndexHedgingEngine.Client.Models.Audit;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class AuditController : Controller, IAuditApi
    {
        private readonly IBalanceOperationService _balanceOperationService;

        public AuditController(IBalanceOperationService balanceOperationService)
        {
            _balanceOperationService = balanceOperationService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of balance operations.</response>
        [HttpGet("balances")]
        [ProducesResponseType(typeof(IReadOnlyCollection<BalanceOperationModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<BalanceOperationModel>> GetBalanceOperationsAsync(DateTime startDate,
            DateTime endDate, int limit, string assetId, BalanceOperationType balanceOperationType)
        {
            var operationType = Mapper.Map<Domain.BalanceOperationType>(balanceOperationType) ;

            IReadOnlyCollection<Domain.BalanceOperation> balanceOperations =
                await _balanceOperationService.GetAsync(startDate, endDate, limit, assetId, operationType);

            return Mapper.Map<List<BalanceOperationModel>>(balanceOperations);
        }
    }
}
