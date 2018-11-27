using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Indices
{
    [UsedImplicitly]
    public class IndexPriceService : IIndexPriceService
    {
        private readonly IIndexPriceRepository _indexPriceRepository;
        private readonly InMemoryCache<IndexPrice> _cache;
        private readonly ILog _log;

        public IndexPriceService(
            IIndexPriceRepository indexPriceRepository,
            ILogFactory logFactory)
        {
            _indexPriceRepository = indexPriceRepository;
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

        public async Task AddAsync(IndexPrice indexPrice)
        {
            IndexPrice currentIndexPrice = await GetByIndexAsync(indexPrice.Name);

            if (currentIndexPrice != null)
                throw new EntityAlreadyExistsException();
            
            await _indexPriceRepository.InsertAsync(indexPrice);

            _cache.Set(indexPrice);
            
            _log.InfoWithDetails("Index price was added", indexPrice);
        }

        public async Task UpdateAsync(IndexPrice indexPrice)
        {
            IndexPrice currentIndexPrice = await GetByIndexAsync(indexPrice.Name);

            if (currentIndexPrice == null)
                throw new EntityNotFoundException();
            
            currentIndexPrice.Update(indexPrice);

            await _indexPriceRepository.UpdateAsync(currentIndexPrice);

            _cache.Set(currentIndexPrice);

            _log.InfoWithDetails("Index price was updated", currentIndexPrice);
        }

        public async Task DeleteAsync(string indexName)
        {
            IndexPrice currentIndexPrice = await GetByIndexAsync(indexName);

            if (currentIndexPrice == null)
                throw new EntityNotFoundException();

            await _indexPriceRepository.DeleteAsync(indexName);

            _cache.Remove(GetKey(indexName));

            _log.InfoWithDetails("Index price was removed", currentIndexPrice);
        }

        private static string GetKey(IndexPrice indexPrice)
            => GetKey(indexPrice.Name);

        private static string GetKey(string indexName)
            => indexName.ToUpper();
    }
}
