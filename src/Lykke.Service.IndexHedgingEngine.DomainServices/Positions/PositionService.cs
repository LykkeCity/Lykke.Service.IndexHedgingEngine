using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Trades;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Positions
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly ILog _log;
        private readonly InMemoryCache<Position> _cache;

        public PositionService(
            IPositionRepository positionRepository,
            ILogFactory logFactory)
        {
            _positionRepository = positionRepository;
            _log = logFactory.CreateLog(this);
            _cache = new InMemoryCache<Position>(GetKey, false);
        }

        public async Task<IReadOnlyCollection<Position>> GetAllAsync()
        {
            IReadOnlyCollection<Position> positions = _cache.GetAll();

            if (positions == null)
            {
                positions = await _positionRepository.GetAllAsync();

                _cache.Initialize(positions);
            }

            return positions;
        }

        public async Task<Position> GetByAssetIdAsync(string assetId, string exchange)
        {
            IReadOnlyCollection<Position> positions = await GetAllAsync();

            return positions.SingleOrDefault(o => o.AssetId == assetId && o.Exchange == exchange);
        }

        public async Task UpdateAsync(string assetId, string exchange, TradeType tradeType, decimal volume,
            decimal oppositeVolume)
        {
            Position position = await GetByAssetIdAsync(assetId, exchange);

            if (position == null)
            {
                int sign = tradeType == TradeType.Sell ? -1 : 1;

                position = Position.Create(assetId, exchange, volume * sign, oppositeVolume * sign * -1);

                await _positionRepository.InsertAsync(position);

                _cache.Set(position);

                _log.InfoWithDetails("Position created", position);
            }
            else
            {
                if (tradeType == TradeType.Sell)
                    position.Decrease(volume, oppositeVolume);
                else
                    position.Increase(volume, oppositeVolume);

                await _positionRepository.UpdateAsync(position);

                _cache.Set(position);

                _log.InfoWithDetails("Position updated", position);
            }
        }

        public async Task CloseAsync(string assetId, string exchange, decimal volume, decimal price)
        {
            Position position = await GetByAssetIdAsync(assetId, exchange);
            
            if(position == null)
                throw new InvalidOperationException("Position not found");
            
            position.Close(volume, price);
            
            await _positionRepository.UpdateAsync(position);
            
            _log.InfoWithDetails("Position closed", position);
        }

        public async Task DeleteAsync(string assetId, string exchange)
        {
            Position position = await GetByAssetIdAsync(assetId, exchange);

            if (position == null)
                throw new EntityNotFoundException();

            await _positionRepository.DeleteAsync(assetId, exchange);

            _cache.Remove(GetKey(position));

            _log.InfoWithDetails("Position deleted", position);
        }

        private static string GetKey(Position position)
            => GetKey(position.AssetId, position.Exchange);

        private static string GetKey(string assetId, string exchange)
            => $"{assetId}_{exchange}".ToUpper();
    }
}
