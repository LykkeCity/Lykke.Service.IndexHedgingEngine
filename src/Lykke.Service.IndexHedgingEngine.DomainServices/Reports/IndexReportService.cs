using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Reports
{
    public class IndexReportService : IIndexReportService
    {
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IIndexPriceService _indexPriceService;
        private readonly ITokenService _tokenService;
        private readonly IBalanceService _balanceService;

        public IndexReportService(
            IIndexSettingsService indexSettingsService,
            IIndexPriceService indexPriceService,
            ITokenService tokenService,
            IBalanceService balanceService)
        {
            _indexSettingsService = indexSettingsService;
            _indexPriceService = indexPriceService;
            _tokenService = tokenService;
            _balanceService = balanceService;
        }

        public async Task<IReadOnlyCollection<IndexReport>> GetAsync()
        {
            IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

            var indexReports = new List<IndexReport>();

            foreach (IndexSettings indexSettings in indicesSettings)
            {
                IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(indexSettings.Name);

                Token token = await _tokenService.GetAsync(indexSettings.AssetId);

                Balance balance = _balanceService.GetByAssetId(ExchangeNames.Lykke, indexSettings.AssetId);

                indexReports.Add(new IndexReport
                {
                    Name = indexSettings.Name,
                    AssetId = indexSettings.AssetId,
                    AssetPairId = indexSettings.AssetPairId,
                    Value = indexPrice?.Value,
                    Price = indexPrice?.Price,
                    Timestamp = indexPrice?.Timestamp,
                    K = indexPrice?.K,
                    Amount = token.Amount,
                    OpenVolume = token.OpenVolume,
                    OppositeVolume = token.OppositeVolume,
                    Balance = balance.Amount,
                    Alpha = indexSettings.Alpha,
                    TrackingFee = indexSettings.TrackingFee,
                    PerformanceFee = indexSettings.PerformanceFee,
                    SellMarkup = indexSettings.SellMarkup,
                    SellVolume = indexSettings.SellVolume,
                    BuyVolume = indexSettings.BuyVolume,
                    Weights = indexPrice?.Weights ?? new AssetWeight[0]
                });
            }

            return indexReports;
        }
    }
}
