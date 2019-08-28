using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices
{
    [UsedImplicitly]
    public class IndexPriceService : IIndexPriceService
    {
        private readonly IIndexPriceRepository _indexPriceRepository;
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly InMemoryCache<IndexPrice> _cache;
        private readonly ILog _log;

        public IndexPriceService(
            IIndexPriceRepository indexPriceRepository,
            IIndexSettingsService indexSettingsService,
            ILogFactory logFactory)
        {
            _indexPriceRepository = indexPriceRepository;
            _indexSettingsService = indexSettingsService;
            _cache = new InMemoryCache<IndexPrice>(GetKey, false);
            _log = logFactory.CreateLog(this);
        }

        public async Task<IReadOnlyCollection<IndexPrice>> GetAllAsync()
        {
            IReadOnlyCollection<IndexPrice> indexStates = _cache.GetAll();

            if (indexStates == null)
            {
                indexStates = await _indexPriceRepository.GetAllAsync();

                _cache.Initialize(indexStates);
            }

            return indexStates;
        }

        public async Task<IndexPrice> GetByIndexAsync(string indexName)
        {
            IReadOnlyCollection<IndexPrice> indexStates = await GetAllAsync();

            return indexStates.SingleOrDefault(o => o.Name == indexName);
        }

        public async Task UpdateAsync(Index index)
        {
            if(!index.ValidateValue())
                throw new InvalidOperationException("Invalid index value");
            
            if(!index.ValidateWeights())
                throw new InvalidOperationException("Invalid index weights");
            
            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(index.Name);

            if (indexSettings == null)
                throw new InvalidOperationException("Index settings not found");

            IndexPrice indexPrice = await GetByIndexAsync(index.Name);

            if (indexPrice == null)
            {
                indexPrice = IndexPrice.Init(index.Name, index.Value, index.Timestamp, index.Weights);

                await _indexPriceRepository.InsertAsync(indexPrice);

                _log.InfoWithDetails("The index price initialized", indexPrice);
            }
            else
            {
                IndexSettlementPrice indexSettlementPrice = IndexSettlementPriceCalculator.Calculate(
                    index.Value, indexPrice.Value, indexSettings.Alpha, indexPrice.K, indexPrice.Price,
                    index.Timestamp, indexPrice.Timestamp, indexSettings.TrackingFee, indexSettings.PerformanceFee,
                    indexSettings.IsShort);
                
                indexPrice.Update(index.Value, index.Timestamp, indexSettlementPrice.Price, indexSettlementPrice.K,
                    indexSettlementPrice.R, indexSettlementPrice.Delta, index.Weights);

                await _indexPriceRepository.UpdateAsync(indexPrice);

                _log.InfoWithDetails("The index price calculated", new
                {
                    indexPrice,
                    indexSettings
                });
            }
            
            _cache.Set(indexPrice);
        }

        private static string GetKey(IndexPrice indexPrice)
            => GetKey(indexPrice.Name);

        private static string GetKey(string indexName)
            => indexName.ToUpper();
    }
}
