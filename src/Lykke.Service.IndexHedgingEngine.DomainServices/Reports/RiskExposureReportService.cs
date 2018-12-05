using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Reports
{
    public class RiskExposureReportService : IRiskExposureReportService
    {
        /// <summary>
        /// Price change used for building report (0.1%).
        /// </summary>
        const decimal DeltaRate = 0.001m;
        
        private readonly IAssetHedgeSettingsService _assetHedgeSettingsService;
        private readonly IIndexPriceService _indexPriceService;
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IInvestmentService _investmentService;
        private readonly IQuoteService _quoteService;
        private readonly IPositionService _positionService;
        private readonly ITokenService _tokenService;

        public RiskExposureReportService(
            IAssetHedgeSettingsService assetHedgeSettingsService,
            IInvestmentService investmentService,
            IIndexSettingsService indexSettingsService,
            IIndexPriceService indexPriceService,
            IPositionService positionService,
            IQuoteService quoteService,
            ITokenService tokenService)
        {
            _assetHedgeSettingsService = assetHedgeSettingsService;
            _indexPriceService = indexPriceService;
            _indexSettingsService = indexSettingsService;
            _investmentService = investmentService;
            _quoteService = quoteService;
            _positionService = positionService;
            _tokenService = tokenService;
        }

        public async Task<RiskExposureReport> GetAsync()
        {
            IReadOnlyCollection<AssetInvestment> investments = _investmentService.GetAll();
            IReadOnlyCollection<IndexSettings> indicesSettings = await _indexSettingsService.GetAllAsync();
            IReadOnlyCollection<IndexPrice> indexPrices = await _indexPriceService.GetAllAsync();
            IReadOnlyCollection<Token> tokens = await _tokenService.GetAllAsync();
            IReadOnlyCollection<AssetHedgeSettings> assetHedgeSettings = await _assetHedgeSettingsService.GetAllAsync();
            IReadOnlyCollection<Position> positions = await _positionService.GetAllAsync();
            
            IReadOnlyCollection<string> assets = indexPrices
                .SelectMany(o => o.Weights?.Select(e => e.AssetId) ?? new string[0])
                .Distinct()
                .ToArray();
            
            decimal totalRemainingAmount = investments.Sum(o => o.RemainingAmount);
            
            return new RiskExposureReport
            {
                UsdCash = totalRemainingAmount,
                Indices = GetIndexReports(indicesSettings, indexPrices, tokens),
                Tokens = GetTokensDelta(indicesSettings, indexPrices, tokens),
                Assets = GetAssetsDelta(assets, assetHedgeSettings, positions)
            };
        }

        private static List<TokenReport> GetIndexReports(IReadOnlyCollection<IndexSettings> indicesSettings,
            IReadOnlyCollection<IndexPrice> indexPrices, IReadOnlyCollection<Token> tokens)
        {
            var tokenReports = new List<TokenReport>();

            foreach (IndexSettings indexSettings in indicesSettings)
            {
                IndexPrice indexPrice = indexPrices.SingleOrDefault(o => o.Name == indexSettings.Name);

                Token token = tokens.SingleOrDefault(o => o.AssetId == indexSettings.AssetId)
                              ?? new Token {AssetId = indexSettings.AssetId};

                tokenReports.Add(new TokenReport
                {
                    Name = indexSettings.Name,
                    AssetId = indexSettings.AssetId,
                    Price = indexPrice?.Price,
                    OpenVolume = token.OpenVolume,
                    Weights = indexPrice?.Weights ?? new AssetWeight[0]
                });
            }

            return tokenReports;
        }

        private static List<InstrumentDeltaReport> GetTokensDelta(IReadOnlyCollection<IndexSettings> indicesSettings,
            IReadOnlyCollection<IndexPrice> indexPrices, IReadOnlyCollection<Token> tokens)
        {
            var tokensDelta = new List<InstrumentDeltaReport>();

            foreach (IndexSettings indexSettings in indicesSettings)
            {
                IndexPrice indexPrice = indexPrices.SingleOrDefault(o => o.Name == indexSettings.Name);

                Token token = tokens.SingleOrDefault(o => o.AssetId == indexSettings.AssetId)
                              ?? new Token {AssetId = indexSettings.AssetId};

                IReadOnlyCollection<AssetWeight> weights = indexPrice?.Weights ?? new AssetWeight[0];

                decimal volume = -token.OpenVolume;
                decimal? volumeInUsd = indexPrice?.Price * volume;

                tokensDelta.Add(new InstrumentDeltaReport
                {
                    Name = indexSettings.Name,
                    AssetId = indexSettings.AssetId,
                    Price = indexPrice?.Price,
                    Volume = volume,
                    VolumeInUsd = volumeInUsd,
                    IsVirtual = false,
                    AssetsDelta = volumeInUsd != null
                        ? ComputeAssetsDelta(volumeInUsd.Value, DeltaRate, weights)
                        : new AssetDelta[0]
                });
            }

            return tokensDelta;
        }

        private List<InstrumentDeltaReport> GetAssetsDelta(IReadOnlyCollection<string> assets,
            IReadOnlyCollection<AssetHedgeSettings> assetHedgeSettings, IReadOnlyCollection<Position> positions)
        {
            var assetsDelta = new List<InstrumentDeltaReport>();

            foreach (string assetId in assets)
            {
                AssetHedgeSettings hedgeSettings = assetHedgeSettings.SingleOrDefault(o => o.AssetId == assetId);

                Quote quote = hedgeSettings != null
                    ? _quoteService.GetByAssetPairId(hedgeSettings.Exchange, hedgeSettings.AssetPairId)
                    : null;

                decimal? price = quote?.Mid;
                decimal volume = positions.Where(o => o.AssetId == assetId).Sum(o => o.Volume);
                decimal? volumeInUsd = quote?.Mid * volume;

                assetsDelta.Add(new InstrumentDeltaReport
                {
                    Name = assetId,
                    AssetId = assetId,
                    Price = price,
                    Volume = volume,
                    VolumeInUsd = volumeInUsd,
                    IsVirtual = hedgeSettings?.Exchange == ExchangeNames.Virtual,
                    AssetsDelta = volumeInUsd != null
                        ? new[] {new AssetDelta {AssetId = assetId, Delta = volumeInUsd.Value * DeltaRate}}
                        : new AssetDelta[0]
                });
            }

            return assetsDelta;
        }

        private static IReadOnlyCollection<AssetDelta> ComputeAssetsDelta(decimal volume, decimal deltaRate,
            IReadOnlyCollection<AssetWeight> weights)
        {
            return weights
                .Select(o =>
                    new AssetDelta
                    {
                        AssetId = o.AssetId,
                        Delta = volume * deltaRate * o.Weight
                    })
                .ToArray();
        }
    }
}
