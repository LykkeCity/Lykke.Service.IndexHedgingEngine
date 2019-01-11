using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Exceptions;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Algorithm;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settlements
{
    public class SettlementService : ISettlementService
    {
        private const decimal AssetMinWeightToDirectTransfer = 0.02m;

        private readonly IIndexPriceService _indexPriceService;
        private readonly IAssetHedgeSettingsService _assetHedgeSettingsService;
        private readonly ISettlementRepository _settlementRepository;
        private readonly IQuoteService _quoteService;
        private readonly IBalanceService _balanceService;
        private readonly ILog _log;

        public SettlementService(
            IIndexPriceService indexPriceService,
            IAssetHedgeSettingsService assetHedgeSettingsService,
            ISettlementRepository settlementRepository,
            IQuoteService quoteService,
            IBalanceService balanceService,
            ILogFactory logFactory)
        {
            _indexPriceService = indexPriceService;
            _assetHedgeSettingsService = assetHedgeSettingsService;
            _settlementRepository = settlementRepository;
            _quoteService = quoteService;
            _balanceService = balanceService;
            _log = logFactory.CreateLog(this);
        }

        public Task<IReadOnlyCollection<Settlement>> GetAllAsync()
        {
            return _settlementRepository.GetAllAsync();
        }

        public Task<IReadOnlyCollection<Settlement>> GetByClientIdAsync(string clientId)
        {
            return _settlementRepository.GetByClientIdAsync(clientId);
        }

        public async Task<Settlement> GetByIdAsync(string settlementId)
        {
            Settlement settlement = await _settlementRepository.GetByIdAsync(settlementId);

            if (settlement == null)
                throw new EntityNotFoundException();

            return settlement;
        }

        public async Task CreateAsync(string indexName, decimal amount, string comment, string walletId,
            string clientId, string userId, bool isDirect)
        {
            IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(indexName);

            if (indexPrice == null)
                throw new InvalidOperationException("Index price not found");

            var settlement = new Settlement
            {
                Id = Guid.NewGuid().ToString("D"),
                IndexName = indexName,
                Amount = amount,
                Price = indexPrice.Price,
                WalletId = walletId,
                ClientId = clientId,
                Comment = comment,
                IsDirect = isDirect,
                Status = SettlementStatus.New,
                CreatedDate = DateTime.UtcNow
            };

            await UpdateAssetSettlementsAsync(settlement, indexPrice.Weights);

            Validate(settlement);

            await _settlementRepository.InsertAsync(settlement);

            _log.InfoWithDetails("Settlement registered", new {settlement, userId});
        }

        public async Task RecalculateAsync(string settlementId, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(settlement.IndexName);

            if (indexPrice == null)
                throw new InvalidOperationException("Index price not found");

            settlement.Price = indexPrice.Price;

            await UpdateAssetSettlementsAsync(settlement, indexPrice.Weights);

            Validate(settlement);

            await _settlementRepository.UpdateAsync(settlement);

            _log.InfoWithDetails("Settlement recalculated", new {settlement, userId});
        }

        public async Task ApproveAsync(string settlementId, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            if (settlement.Status == SettlementStatus.New)
                throw new InvalidOperationException("Only new settlement can be approved");

            await _settlementRepository.UpdateStatusAsync(settlement.Id, SettlementStatus.Approved);

            _log.InfoWithDetails("Settlement approved", new {settlement.Id, userId});
        }

        public async Task RejectAsync(string settlementId, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            if (settlement.Status == SettlementStatus.New)
                throw new InvalidOperationException("Only new settlement can be rejected");

            await _settlementRepository.UpdateStatusAsync(settlement.Id, SettlementStatus.Rejected);

            _log.InfoWithDetails("Settlement rejected", new {settlement.Id, userId});
        }

        private async Task UpdateAssetSettlementsAsync(Settlement settlement,
            IReadOnlyCollection<AssetWeight> assetWeights)
        {
            IReadOnlyDictionary<string, Quote> assetPrices =
                await GetAssetPricesAsync(assetWeights.Select(o => o.AssetId));

            IReadOnlyDictionary<string, decimal> weights = assetWeights
                .Where(o => !o.IsDisabled)
                .ToDictionary(key => key.AssetId, value => value.Weight);

            IReadOnlyCollection<AssetSettlementAmount> assetAmounts =
                AssetSettlementCalculator.Calculate(settlement.Amount, settlement.Price, weights, assetPrices);

            var assetSettlements = new List<AssetSettlement>();

            foreach (AssetSettlementAmount assetSettlementAmount in assetAmounts)
            {
                AssetHedgeSettings assetHedgeSettings =
                    await _assetHedgeSettingsService.EnsureAsync(assetSettlementAmount.AssetId);

                assetSettlements.Add(new AssetSettlement
                {
                    AssetId = assetSettlementAmount.AssetId,
                    SettlementId = settlement.Id,
                    Amount = assetSettlementAmount.Amount,
                    Price = assetSettlementAmount.Price,
                    Fee = decimal.Zero,
                    Weight = assetSettlementAmount.Weight,
                    IsDirect = settlement.IsDirect || assetSettlementAmount.Weight <= AssetMinWeightToDirectTransfer,
                    IsExternal = assetHedgeSettings.Exchange == ExchangeNames.Lykke,
                    Status = AssetSettlementStatus.New,
                    ActualAmount = assetSettlementAmount.Amount,
                    ActualPrice = assetSettlementAmount.Price
                });
            }

            settlement.Assets = assetSettlements;
        }

        private void Validate(Settlement settlement)
        {
            AssetSettlement[] assetSettlementsDirect = settlement.Assets
                .Where(o => o.IsDirect && !o.IsExternal)
                .ToArray();

            AssetSettlement[] assetSettlementsInUsd = settlement.Assets
                .Where(o => !o.IsDirect)
                .ToArray();

            foreach (AssetSettlement assetSettlement in assetSettlementsDirect)
            {
                Balance balance = _balanceService.GetByAssetId(assetSettlement.AssetId, ExchangeNames.Lykke);

                if (balance.Amount - balance.Reserved < assetSettlement.Amount)
                {
                    assetSettlement.Status = AssetSettlementStatus.Error;
                    assetSettlement.Error = "No enough fonds";
                }
            }

            decimal amountInUsd = assetSettlementsInUsd.Sum(o => o.Amount * o.Price);

            Balance usdBalance = _balanceService.GetByAssetId("USD", ExchangeNames.Lykke);

            if (usdBalance.Amount - usdBalance.Reserved < amountInUsd)
            {
                foreach (AssetSettlement assetSettlement in assetSettlementsDirect)
                {
                    assetSettlement.Status = AssetSettlementStatus.Error;
                    assetSettlement.Error = "No enough fonds";
                }
            }
        }

        private async Task<IReadOnlyDictionary<string, Quote>> GetAssetPricesAsync(IEnumerable<string> assets)
        {
            var assetPrices = new Dictionary<string, Quote>();

            foreach (string assetId in assets)
            {
                AssetHedgeSettings assetHedgeSettings = await _assetHedgeSettingsService.EnsureAsync(assetId);

                Quote quote = _quoteService.GetByAssetPairId(assetHedgeSettings.Exchange,
                    assetHedgeSettings.AssetPairId);

                if (quote != null)
                    assetPrices[assetId] = quote;
            }

            return assetPrices;
        }
    }
}
