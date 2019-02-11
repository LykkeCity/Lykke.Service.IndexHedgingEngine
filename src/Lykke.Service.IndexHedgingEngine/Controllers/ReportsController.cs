using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models;
using Lykke.Service.IndexHedgingEngine.Client.Models.Reports;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Balances;
using Lykke.Service.IndexHedgingEngine.DomainServices.Hedging;
using Lykke.Service.IndexHedgingEngine.DomainServices.Indices;
using Lykke.Service.IndexHedgingEngine.DomainServices.OrderBooks;
using Lykke.Service.IndexHedgingEngine.DomainServices.Positions;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class ReportsController : Controller, IReportsApi
    {
        private readonly IPositionReportService _positionReportService;
        private readonly IIndexReportService _indexReportService;
        private readonly IRiskExposureReportService _riskExposureReportService;

        public ReportsController(
            IPositionReportService positionReportService,
            IIndexReportService indexReportService,
            IRiskExposureReportService riskExposureReportService)
        {
            _positionReportService = positionReportService;
            _indexReportService = indexReportService;
            _riskExposureReportService = riskExposureReportService;
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
        /// <response code="200">A collection of position reports.</response>
        [HttpGet("positions/external")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PositionReportModel>), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<PositionReportModel>> GetExternalPositionReportsAsync(string exchange)
        {
            IReadOnlyCollection<PositionReport> positionReports =
                await _positionReportService.GetByExchangeAsync(exchange);

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
        
        /// <inheritdoc/>
        /// <response code="200">A risk exposure report.</response>
        [HttpGet("riskexposure")]
        [ProducesResponseType(typeof(RiskExposureReportModel), (int) HttpStatusCode.OK)]
        public async Task<RiskExposureReportModel> GetRiskExposureReportsAsync()
        {
            RiskExposureReport riskExposureReport = await _riskExposureReportService.GetAsync();

            return Mapper.Map<RiskExposureReportModel>(riskExposureReport);
        }
    }
}
