using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Reports;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class ReportsController : Controller, IReportsApi
    {
        private readonly IPositionReportService _positionReportService;
        private readonly IIndexReportService _indexReportService;

        public ReportsController(
            IPositionReportService positionReportService,
            IIndexReportService indexReportService)
        {
            _positionReportService = positionReportService;
            _indexReportService = indexReportService;
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of position reports.</response>
        [HttpGet("positions")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PositionReportModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<PositionReportModel>> GetPositionReportsAsync()
        {
            IReadOnlyCollection<PositionReport> positionReports = await _positionReportService.GetAsync();

            return Mapper.Map<PositionReportModel[]>(positionReports);
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of position reports.</response>
        [HttpGet("positions/lykke")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PositionReportModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<PositionReportModel>> GetLykkePositionReportsAsync()
        {
            IReadOnlyCollection<PositionReport> positionReports =
                await _positionReportService.GetByExchangeAsync(ExchangeNames.Lykke);

            return Mapper.Map<PositionReportModel[]>(positionReports);
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of position reports.</response>
        [HttpGet("positions/virtual")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PositionReportModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<PositionReportModel>> GetVirtualPositionReportsAsync()
        {
            IReadOnlyCollection<PositionReport> positionReports =
                await _positionReportService.GetByExchangeAsync(ExchangeNames.Virtual);

            return Mapper.Map<PositionReportModel[]>(positionReports);
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of index reports.</response>
        [HttpGet("indices")]
        [ProducesResponseType(typeof(IReadOnlyCollection<IndexReportModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<IndexReportModel>> GetIndexReportsAsync()
        {
            IReadOnlyCollection<IndexReport> indexReports = await _indexReportService.GetAsync();

            return Mapper.Map<IndexReportModel[]>(indexReports);
        }
    }
}
