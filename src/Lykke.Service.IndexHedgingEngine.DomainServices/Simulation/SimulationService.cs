using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Simulation;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Simulation
{
    public class SimulationService : ISimulationService
    {
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IAssetHedgeSettingsService _assetHedgeSettingsService;
        private readonly IIndexPriceService _indexPriceService;
        private readonly IQuoteService _quoteService;
        private readonly ISimulationParametersRepository _simulationParametersRepository;

        public SimulationService(
            IIndexSettingsService indexSettingsService,
            IAssetHedgeSettingsService assetHedgeSettingsService,
            IIndexPriceService indexPriceService,
            IQuoteService quoteService,
            ISimulationParametersRepository simulationParametersRepository)
        {
            _indexSettingsService = indexSettingsService;
            _assetHedgeSettingsService = assetHedgeSettingsService;
            _indexPriceService = indexPriceService;
            _quoteService = quoteService;
            _simulationParametersRepository = simulationParametersRepository;
        }

        public async Task<SimulationReport> GetReportAsync(string indexName)
        {
            SimulationParameters simulationParameters =
                await _simulationParametersRepository.GetByIndexNameAsync(indexName);

            if (simulationParameters == null)
                simulationParameters = SimulationParameters.Create(indexName);
            
            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(indexName);

            if(indexSettings == null)
                return null;
            
            IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(indexSettings.Name);
            
            var simulationReport = new SimulationReport
            {
                IndexName = indexName,
                AssetId = indexSettings.AssetId,
                AssetPairId = indexSettings.AssetPairId,
                Value = indexPrice?.Value,
                Price = indexPrice?.Price,
                Timestamp = indexPrice?.Timestamp,
                K = indexPrice?.K,
                OpenTokens = simulationParameters.OpenTokens,
                Investments = simulationParameters.Investments,
                Alpha = indexSettings.Alpha,
                TrackingFee = indexSettings.TrackingFee,
                PerformanceFee = indexSettings.PerformanceFee,
                SellMarkup = indexSettings.SellMarkup,
                SellVolume = indexSettings.SellVolume,
                BuyVolume = indexSettings.BuyVolume
            };
            
            var assets = new List<AssetDistribution>();

            if (indexPrice != null)
            {
                foreach (AssetWeight assetWeight in indexPrice.Weights)
                {
                    AssetHedgeSettings assetHedgeSettings =
                        await _assetHedgeSettingsService.EnsureAsync(assetWeight.AssetId);

                    Quote quote = _quoteService.GetByAssetPairId(assetHedgeSettings.Exchange,
                        assetHedgeSettings.AssetPairId);

                    decimal amountInUsd = simulationParameters.Investments * assetWeight.Weight;

                    decimal amount = amountInUsd / quote?.Mid ?? decimal.Zero;
                    
                    assets.Add(new AssetDistribution
                    {
                        Asset = assetWeight.AssetId,
                        Weight = assetWeight.Weight,
                        IsHedged = simulationParameters.Assets.Contains(assetWeight.AssetId),
                        Amount = amount,
                        AmountInUsd = amountInUsd,
                        Quote = quote
                    });
                }
            }

            simulationReport.Assets = assets;
            
            return simulationReport;
        }

        public async Task UpdateParametersAsync(string indexName, decimal openTokens, decimal investments)
        {
            SimulationParameters simulationParameters =
                await _simulationParametersRepository.GetByIndexNameAsync(indexName);

            if (simulationParameters == null)
                simulationParameters = SimulationParameters.Create(indexName);

            simulationParameters.Update(openTokens, investments);
            
            await _simulationParametersRepository.SaveAsync(simulationParameters);
        }

        public async Task AddAssetAsync(string indexName, string asset)
        {
            SimulationParameters simulationParameters =
                await _simulationParametersRepository.GetByIndexNameAsync(indexName);
            
            if(simulationParameters == null)
                throw new EntityNotFoundException();
            
            simulationParameters.AddAsset(asset);

            await _simulationParametersRepository.SaveAsync(simulationParameters);
        }
        
        public async Task RemoveAssetAsync(string indexName, string asset)
        {
            SimulationParameters simulationParameters =
                await _simulationParametersRepository.GetByIndexNameAsync(indexName);
            
            if(simulationParameters == null)
                throw new EntityNotFoundException();
            
            simulationParameters.RemoveAsset(asset);

            await _simulationParametersRepository.SaveAsync(simulationParameters);
        }
    }
}
