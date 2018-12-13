using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Reports
{
    public class PositionReportService : IPositionReportService
    {
        private readonly IPositionService _positionService;
        private readonly IHedgeLimitOrderService _hedgeLimitOrderService;
        private readonly IAssetHedgeSettingsService _assetHedgeSettingsService;
        private readonly IInvestmentService _investmentService;
        private readonly IRateService _rateService;

        public PositionReportService(
            IPositionService positionService,
            IHedgeLimitOrderService hedgeLimitOrderService,
            IAssetHedgeSettingsService assetHedgeSettingsService,
            IInvestmentService investmentService,
            IRateService rateService)
        {
            _positionService = positionService;
            _hedgeLimitOrderService = hedgeLimitOrderService;
            _assetHedgeSettingsService = assetHedgeSettingsService;
            _investmentService = investmentService;
            _rateService = rateService;
        }

        public Task<IReadOnlyCollection<PositionReport>> GetAsync()
        {
            return CreateReports();
        }

        public async Task<IReadOnlyCollection<PositionReport>> GetByExchangeAsync(string exchange)
        {
            IReadOnlyCollection<PositionReport> positionReports = await CreateReports();

            return positionReports.Where(o => o.Exchange == exchange).ToArray();
        }

        private async Task<IReadOnlyCollection<PositionReport>> CreateReports()
        {
            IReadOnlyCollection<Position> positions = await _positionService.GetAllAsync();

            IReadOnlyCollection<AssetInvestment> assetInvestments = _investmentService.GetAll();

            IReadOnlyCollection<HedgeLimitOrder> hedgeLimitOrders = _hedgeLimitOrderService.GetAll();

            IReadOnlyCollection<AssetHedgeSettings>
                assetsHedgeSettings = await _assetHedgeSettingsService.GetAllAsync();

            string[] assets = positions.Select(o => o.AssetId)
                .Union(assetInvestments.Select(o => o.AssetId))
                .Union(assetsHedgeSettings.Select(o => o.AssetId))
                .ToArray();

            var positionReports = new List<PositionReport>();

            foreach (string assetId in assets)
            {
                AssetHedgeSettings assetHedgeSettings = await _assetHedgeSettingsService.EnsureAsync(assetId);

                Position currentPosition = positions
                    .SingleOrDefault(o => o.AssetId == assetId && o.Exchange == assetHedgeSettings.Exchange);

                HedgeLimitOrder hedgeLimitOrder = hedgeLimitOrders.SingleOrDefault(o => o.AssetId == assetId);

                AssetInvestment assetInvestment = assetInvestments.SingleOrDefault(o => o.AssetId == assetId);

                decimal? volumeInUsd = null;

                if (currentPosition != null)
                {
                    volumeInUsd = GetVolumeInUsd(currentPosition.AssetId, currentPosition.Exchange,
                        currentPosition.Volume);
                }

                positionReports.Add(new PositionReport
                {
                    AssetId = assetId,
                    Exchange = assetHedgeSettings.Exchange,
                    Quote = assetInvestment?.Quote,
                    Volume = currentPosition?.Volume,
                    VolumeInUsd = volumeInUsd,
                    OppositeVolume = currentPosition?.OppositeVolume,
                    PnL = volumeInUsd.HasValue ? currentPosition.OppositeVolume + volumeInUsd : null,
                    HedgeLimitOrder = hedgeLimitOrder,
                    AssetInvestment = assetInvestment,
                    Error = ValidateAssetHedgeSettings(assetHedgeSettings)
                            ?? ValidateInvestments(assetInvestment)
                            ?? ValidateQuote(assetInvestment?.Quote)
                });

                IEnumerable<Position> otherPositions = positions
                    .Where(o => o.AssetId == assetId && o.Exchange != assetHedgeSettings.Exchange);

                foreach (Position position in otherPositions)
                {
                    Quote quote = _rateService.GetQuoteUsd(position.AssetId, position.Exchange);

                    volumeInUsd = GetVolumeInUsd(position.AssetId, position.Exchange, position.Volume);

                    positionReports.Add(new PositionReport
                    {
                        AssetId = assetId,
                        Exchange = position.Exchange,
                        Quote = quote,
                        Volume = position.Volume,
                        VolumeInUsd = volumeInUsd,
                        OppositeVolume = position.OppositeVolume,
                        PnL = volumeInUsd.HasValue ? position.OppositeVolume + volumeInUsd : null,
                        HedgeLimitOrder = null,
                        AssetInvestment = null,
                        Error = ValidateAssetHedgeSettings(assetHedgeSettings)
                                ?? ValidateQuote(quote)
                    });
                }
            }

            return positionReports
                .OrderBy(o => o.AssetId)
                .ToArray();
        }

        private decimal? GetVolumeInUsd(string assetId, string exchange, decimal volume)
        {
            if (_rateService.TryConvertToUsd(assetId, exchange, volume, out decimal volumeInUsd))
                return volumeInUsd;

            return null;
        }

        private static string ValidateAssetHedgeSettings(AssetHedgeSettings assetHedgeSettings)
        {
            if (assetHedgeSettings != null)
            {
                if (!assetHedgeSettings.Approved)
                    return "Asset hedge settings not approved";

                if (assetHedgeSettings.Mode == AssetHedgeMode.Disabled)
                    return "Asset hedging disabled";
            }
            else
            {
                return "No settings";
            }
            
            return null;
        }

        private static string ValidateInvestments(AssetInvestment assetInvestment)
        {
            if(assetInvestment?.IsDisabled == true)
                return "Asset disabled";

            return null;
        }

        private static string ValidateQuote(Quote quote)
        {
            if (quote == null)
                return "No quote";

            return null;
        }
    }
}
