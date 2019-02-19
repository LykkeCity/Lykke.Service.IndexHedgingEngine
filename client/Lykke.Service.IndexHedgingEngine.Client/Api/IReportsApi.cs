using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Reports;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with reports.
    /// </summary>
    [PublicAPI]
    public interface IReportsApi
    {
        /// <summary>
        /// Returns reports by positions. 
        /// </summary>
        /// <returns>A collection of position reports.</returns>
        [Get("/api/Reports/positions")]
        Task<IReadOnlyCollection<PositionReportModel>> GetPositionReportsAsync();

        /// <summary>
        /// Returns report by positions on Lykke exchange. 
        /// </summary>
        /// <returns>A collection of position reports.</returns>
        [Get("/api/Reports/positions/lykke")]
        Task<IReadOnlyCollection<PositionReportModel>> GetLykkePositionReportsAsync();

        /// <summary>
        /// Returns report by positions on virtual exchange. 
        /// </summary>
        /// <returns>A collection of position reports.</returns>
        [Get("/api/Reports/positions/virtual")]
        Task<IReadOnlyCollection<PositionReportModel>> GetVirtualPositionReportsAsync();

        /// <summary>
        /// Returns report by positions on external exchange. 
        /// </summary>
        /// <returns>A collection of position reports.</returns>
        [Get("/api/Reports/positions/external")]
        Task<IReadOnlyCollection<PositionReportModel>> GetExternalPositionReportsAsync(string exchange);

        /// <summary>
        /// Returns reports by indices. 
        /// </summary>
        /// <returns>A collection of of index reports.</returns>
        [Get("/api/Reports/indices")]
        Task<IReadOnlyCollection<IndexReportModel>> GetIndexReportsAsync();

        /// <summary>
        /// Returns risk exposure report.
        /// </summary>
        /// <returns>A risk exposure report.</returns>
        [Get("/api/Reports/riskexposure")]
        Task<RiskExposureReportModel> GetRiskExposureReportsAsync();

        /// <summary>
        /// Returns profit loss report.
        /// </summary>
        /// <returns>A profit loss report.</returns>
        [Get("/api/Reports/pnl")]
        Task<ProfitLossReportModel> GetProfitLossReportAsync();
    }
}
