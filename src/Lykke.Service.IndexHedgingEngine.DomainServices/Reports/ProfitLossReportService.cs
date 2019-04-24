using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Reports;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Reports
{
    public class ProfitLossReportService : IProfitLossReportService
    {
        private readonly IIndexPriceService _indexPriceService;
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly ITokenService _tokenService;
        private readonly IPositionService _positionService;
        private readonly IRateService _rateService;

        public ProfitLossReportService(
            IIndexPriceService indexPriceService,
            IIndexSettingsService indexSettingsService,
            ITokenService tokenService,
            IPositionService positionService,
            IRateService rateService)
        {
            _indexPriceService = indexPriceService;
            _indexSettingsService = indexSettingsService;
            _tokenService = tokenService;
            _positionService = positionService;
            _rateService = rateService;
        }

        public async Task<ProfitLossReport> GetAsync()
        {
            IReadOnlyCollection<Position> positions = await _positionService.GetAllAsync();

            decimal positionsVolumeInUsd = 0;
            decimal positionsOppositeVolume = 0;

            foreach (Position position in positions.Where(position => position.Exchange != ExchangeNames.Virtual))
            {
                positionsOppositeVolume += position.OppositeVolume;
                    
                if (_rateService.TryConvertToUsd(position.AssetId, position.Exchange, position.Volume,
                    out decimal amountInUsd))
                {
                    positionsVolumeInUsd += amountInUsd;
                }
            }

            decimal investments = 0;
            decimal openTokensAmountInUsd = 0;

            IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();

            foreach (IndexSettings indexSettings in indicesSettings)
            {
                IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(indexSettings.Name);
                Token token = await _tokenService.GetAsync(indexSettings.AssetId);

                investments += token?.OppositeVolume ?? 0;
                openTokensAmountInUsd += token?.OpenVolume * indexPrice?.Price ?? 0;
            }

            decimal balance = positionsVolumeInUsd + (investments - Math.Abs(positionsOppositeVolume));
            decimal pnl = openTokensAmountInUsd - balance;

            return new ProfitLossReport(balance, pnl);
        }
    }
}
