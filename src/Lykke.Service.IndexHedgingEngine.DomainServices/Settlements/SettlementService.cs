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
        private readonly IIndexSettingsService _indexSettingsService;
        private readonly IAssetHedgeSettingsService _assetHedgeSettingsService;
        private readonly ISettlementRepository _settlementRepository;
        private readonly IQuoteService _quoteService;
        private readonly IBalanceService _balanceService;
        private readonly ISettingsService _settingsService;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly IInstrumentService _instrumentService;
        private readonly IPositionService _positionService;
        private readonly ITokenService _tokenService;
        private readonly ILog _log;

        public SettlementService(
            IIndexPriceService indexPriceService,
            IIndexSettingsService indexSettingsService,
            IAssetHedgeSettingsService assetHedgeSettingsService,
            ISettlementRepository settlementRepository,
            IQuoteService quoteService,
            IBalanceService balanceService,
            ISettingsService settingsService,
            ILykkeExchangeService lykkeExchangeService,
            IInstrumentService instrumentService,
            IPositionService positionService,
            ITokenService tokenService,
            ILogFactory logFactory)
        {
            _indexPriceService = indexPriceService;
            _indexSettingsService = indexSettingsService;
            _assetHedgeSettingsService = assetHedgeSettingsService;
            _settlementRepository = settlementRepository;
            _quoteService = quoteService;
            _balanceService = balanceService;
            _settingsService = settingsService;
            _lykkeExchangeService = lykkeExchangeService;
            _instrumentService = instrumentService;
            _positionService = positionService;
            _tokenService = tokenService;
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

        public async Task ExecuteAsync()
        {
            IReadOnlyCollection<Settlement> settlements = await _settlementRepository.GetActiveAsync();

            foreach (Settlement settlement in settlements.Where(o => o.Error == SettlementError.None))
            {
                try
                {
                    if (settlement.Status == SettlementStatus.Approved)
                    {
                        IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                            .Where(o => !o.IsManual())
                            .Where(o => o.Status == AssetSettlementStatus.New)
                            .Where(o => o.Error == SettlementError.None);

                        foreach (AssetSettlement assetSettlement in assetSettlements)
                            await ReserveAssetAsync(assetSettlement, settlement.ClientId);

                        bool reserved = settlement.Assets
                            .Where(o => !o.IsManual())
                            .All(o => o.Status == AssetSettlementStatus.Reserved);
                        
                        if (reserved)
                            await ReserveTokenAsync(settlement);
                    }

                    if (settlement.Status == SettlementStatus.Reserved)
                    {
                        IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                            .Where(o => !o.IsManual())
                            .Where(o => o.Status == AssetSettlementStatus.Reserved)
                            .Where(o => o.Error == SettlementError.None);

                        foreach (AssetSettlement assetSettlement in assetSettlements)
                            await TransferAssetAsync(assetSettlement, settlement.ClientId, settlement.WalletId);

                        bool transferred = settlement.Assets
                            .Where(o => !o.IsManual())
                            .All(o => o.Status == AssetSettlementStatus.Transferred);
                        
                        if (transferred)
                            await TransferTokenAsync(settlement);
                    }

                    if (settlement.Status == SettlementStatus.Transferred)
                    {
                        IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                            .Where(o => o.Status == AssetSettlementStatus.Transferred)
                            .Where(o => o.Error == SettlementError.None);
                        
                        foreach (AssetSettlement assetSettlement in assetSettlements)
                            await CompleteAssetAsync(assetSettlement);

                        bool completed = settlement.Assets.All(o => o.Status == AssetSettlementStatus.Completed);
                        
                        if (completed)
                            await CompleteTokenAsync(settlement);
                    }
                }
                catch (Exception exception)
                {
                    _log.WarningWithDetails("An error occurred while processing settlement", exception, settlement);
                }
            }
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
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            await UpdateAssetsAsync(settlement, indexPrice.Weights);

            await ValidateBalanceAsync(settlement);

            await _settlementRepository.InsertAsync(settlement);

            _log.InfoWithDetails("Settlement registered", settlement);
        }

        public async Task RecalculateAsync(string settlementId, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            if (settlement.Status != SettlementStatus.New)
                throw new InvalidOperationException("Only new settlement can be recalculated");

            IndexPrice indexPrice = await _indexPriceService.GetByIndexAsync(settlement.IndexName);

            if (indexPrice == null)
                throw new InvalidOperationException("Index price not found");

            settlement.Price = indexPrice.Price;

            await UpdateAssetsAsync(settlement, indexPrice.Weights);

            await ValidateBalanceAsync(settlement);

            await _settlementRepository.ReplaceAsync(settlement);

            _log.InfoWithDetails("Settlement recalculated", new {settlement, userId});
        }

        public async Task ApproveAsync(string settlementId, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            if (settlement.Status != SettlementStatus.New)
                throw new InvalidOperationException("Only new settlement can be approved");

            settlement.Status = SettlementStatus.Approved;

            await _settlementRepository.UpdateAsync(settlement);

            _log.InfoWithDetails("Settlement approved", new {settlement.Id, userId});
        }

        public async Task RejectAsync(string settlementId, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            var allowedStatuses = new[] {SettlementStatus.New, SettlementStatus.Approved, SettlementStatus.Reserved};

            bool hasTransferredAssets = settlement.Assets
                .Any(o => o.Status == AssetSettlementStatus.Reserved || o.Status == AssetSettlementStatus.Completed);

            if (!allowedStatuses.Contains(settlement.Status) || hasTransferredAssets)
                throw new InvalidOperationException("Settlement can not be rejected");

            IEnumerable<AssetSettlement> reservedAssets = settlement.Assets
                .Where(o => o.Status == AssetSettlementStatus.Reserved);

            foreach (AssetSettlement assetSettlement in reservedAssets)
            {
                assetSettlement.Error = await TransferAsync(assetSettlement, _settingsService.GetTransitWalletId(),
                    _settingsService.GetWalletId(),
                    $"Rollback reserved for client. ClientId: {settlement.ClientId}; SettlementId: {assetSettlement.SettlementId}");
                
                if (assetSettlement.Error == SettlementError.None)
                    assetSettlement.Status = AssetSettlementStatus.Cancelled;

                await _settlementRepository.UpdateAsync(assetSettlement);
            }

            if (settlement.Status == SettlementStatus.Reserved)
            {
                IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(settlement.IndexName);

                settlement.Error = await TransferAsync(_settingsService.GetTransitWalletId(), settlement.WalletId,
                    indexSettings.AssetId, settlement.Amount,
                    $"Rollback reserved for market maker. ClientId: {settlement.ClientId}; SettlementId: {settlement.Id}");

                if (settlement.Error == SettlementError.None)
                    settlement.Status = SettlementStatus.Rejected;

                await _settlementRepository.UpdateAsync(settlement);
            }

            _log.InfoWithDetails("Settlement rejected", new {settlement.Id, userId});
        }

        public async Task UpdateAssetAsync(string settlementId, string assetId, decimal amount, bool isDirect,
            bool isExternal, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            if (settlement.Status != SettlementStatus.New)
                throw new InvalidOperationException("Only new settlement can be updated");

            AssetSettlement assetSettlement = settlement.GetAsset(assetId);

            if (assetSettlement == null)
                throw new InvalidOperationException("Asset not found");

            assetSettlement.Update(amount, isDirect, isExternal);

            await ValidateBalanceAsync(settlement);

            await _settlementRepository.ReplaceAsync(settlement);

            _log.InfoWithDetails("Asset updated", new {assetSettlement, userId});
        }

        public async Task RetryAsync(string settlementId, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            // TODO: Validate status
            
            settlement.Error = SettlementError.None;

            await _settlementRepository.UpdateAsync(settlement);
            
            _log.InfoWithDetails("Settlement retry", new {settlement, userId});
        }

        public async Task RetryAssetAsync(string settlementId, string assetId, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            AssetSettlement assetSettlement = settlement.GetAsset(assetId);

            if (assetSettlement == null)
                throw new InvalidOperationException("Asset not found");

            switch (settlement.Status)
            {
                case SettlementStatus.Approved:
                case SettlementStatus.Reserved:
                    assetSettlement.Error = SettlementError.None;
                    break;
                default:
                    throw new InvalidOperationException("Can not retry asset.");
            }

            await _settlementRepository.UpdateAsync(assetSettlement);

            _log.InfoWithDetails("Asset updated", new {assetSettlement, userId});
        }

        public async Task ValidateAsync(string settlementId, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            if (settlement.Status != SettlementStatus.New)
                throw new InvalidOperationException("Only new settlement can be validated");

            await ValidateBalanceAsync(settlement);

            await _settlementRepository.ReplaceAsync(settlement);

            _log.InfoWithDetails("Settlement validated", new {settlement.Id, userId});
        }

        public async Task ExecuteAssetAsync(string settlementId, string assetId, decimal actualAmount,
            decimal actualPrice, string userId)
        {
            Settlement settlement = await GetByIdAsync(settlementId);

            AssetSettlement assetSettlement = settlement.GetAsset(assetId);

            if (assetSettlement == null)
                throw new InvalidOperationException("Asset not found");

            if (!assetSettlement.IsDirect || !assetSettlement.IsExternal)
                throw new InvalidOperationException("Only direct external assets can be manually executed");

            switch (settlement.Status)
            {
                case SettlementStatus.Approved:
                case SettlementStatus.Reserved:
                    assetSettlement.ActualAmount = actualAmount;
                    assetSettlement.ActualPrice = actualPrice;
                    assetSettlement.Status = AssetSettlementStatus.Transferred;
                    break;
                default:
                    throw new InvalidOperationException("Can not execute asset.");
            }

            await _settlementRepository.UpdateAsync(assetSettlement);

            _log.InfoWithDetails("Asset updated", new {assetSettlement, userId});
        }

        private async Task ReserveAssetAsync(AssetSettlement assetSettlement, string clientId)
        {
            assetSettlement.Error = await TransferAsync(assetSettlement, _settingsService.GetWalletId(),
                _settingsService.GetTransitWalletId(),
                $"Reserve for client. ClientId: {clientId}; SettlementId: {assetSettlement.SettlementId}");

            if (assetSettlement.Error == SettlementError.None)
                assetSettlement.Status = AssetSettlementStatus.Reserved;

            await _settlementRepository.UpdateAsync(assetSettlement);
        }

        private async Task ReserveTokenAsync(Settlement settlement)
        {
            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(settlement.IndexName);

            AssetSettings assetSettings = (await _instrumentService.GetAssetsAsync())
                .Single(o => o.Exchange == ExchangeNames.Lykke && o.AssetId == indexSettings.AssetId);

            settlement.Error = await TransferAsync(settlement.WalletId, _settingsService.GetTransitWalletId(),
                assetSettings.Asset, settlement.Amount,
                $"Reserve for market maker. ClientId: {settlement.ClientId}; SettlementId: {settlement.Id}");

            if (settlement.Error == SettlementError.None)
                settlement.Status = SettlementStatus.Reserved;

            await _settlementRepository.UpdateAsync(settlement);
        }

        private async Task TransferAssetAsync(AssetSettlement assetSettlement, string clientId, string walletId)
        {
            assetSettlement.Error = await TransferAsync(assetSettlement,
                _settingsService.GetTransitWalletId(), walletId,
                $"Transfer to client. ClientId: {clientId}; SettlementId: {assetSettlement.SettlementId}");

            if (assetSettlement.Error == SettlementError.None)
                assetSettlement.Status = AssetSettlementStatus.Transferred;

            await _settlementRepository.UpdateAsync(assetSettlement);
        }

        private async Task TransferTokenAsync(Settlement settlement)
        {
            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(settlement.IndexName);

            AssetSettings assetSettings = (await _instrumentService.GetAssetsAsync())
                .Single(o => o.Exchange == ExchangeNames.Lykke && o.AssetId == indexSettings.AssetId);
            
            settlement.Error = await TransferAsync(_settingsService.GetTransitWalletId(),
                _settingsService.GetWalletId(), assetSettings.Asset, settlement.Amount,
                $"Transfer to market maker. ClientId: {settlement.ClientId}; SettlementId: {settlement.Id}");

            if (settlement.Error == SettlementError.None)
                settlement.Status = SettlementStatus.Transferred;

            await _settlementRepository.UpdateAsync(settlement);
        }

        private async Task CompleteAssetAsync(AssetSettlement assetSettlement)
        {
            try
            {
                AssetHedgeSettings assetHedgeSettings =
                    await _assetHedgeSettingsService.GetByAssetIdAsync(assetSettlement.AssetId);

                if (!assetSettlement.IsDirect && !assetSettlement.IsExternal)
                {
                    // In this case position will be closed automatically by hedge limit order.
                }
                else
                {
                    await _positionService.CloseAsync(assetSettlement.AssetId, assetHedgeSettings.Exchange,
                        assetSettlement.ActualAmount, assetSettlement.ActualPrice);
                }

                assetSettlement.Status = AssetSettlementStatus.Completed;
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, "An error occurred while completing asset settlement",
                    assetSettlement);

                assetSettlement.Error = SettlementError.Unknown;
            }

            await _settlementRepository.UpdateAsync(assetSettlement);
        }

        private async Task CompleteTokenAsync(Settlement settlement)
        {
            try
            {
                IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(settlement.IndexName);

                await _tokenService.CloseAsync(indexSettings.AssetId, settlement.Amount, settlement.Price);

                settlement.Status = SettlementStatus.Completed;
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, "An error occurred while completing settlement", settlement);

                settlement.Error = SettlementError.Unknown;
            }

            await _settlementRepository.UpdateAsync(settlement);
        }

        private async Task<SettlementError> TransferAsync(AssetSettlement assetSettlement, string sourceWalletId,
            string targetWalletId, string comment)
        {
            string assetId;
            decimal amount;

            if (assetSettlement.IsDirect)
            {
                assetId = assetSettlement.AssetId;
                amount = assetSettlement.Amount;
            }
            else
            {
                assetId = "USD";
                amount = assetSettlement.Amount * assetSettlement.Price;
            }

            return await TransferAsync(sourceWalletId, targetWalletId, assetId, amount, comment);
        }

        private async Task<SettlementError> TransferAsync(string sourceWalletId, string targetWalletId, string assetId,
            decimal amount, string comment)
        {
            AssetSettings assetSettings = await _instrumentService.GetAssetAsync(assetId, ExchangeNames.Lykke);

            if (assetSettings == null)
            {
                _log.WarningWithDetails("No asset setting", assetId);
                return SettlementError.Unknown;
            }

            try
            {
                string cashOutTransactionId = await _lykkeExchangeService.CashOutAsync(sourceWalletId,
                    assetSettings.AssetId, amount, "settlement", comment);

                _log.InfoWithDetails("Cash out", new
                {
                    Comment = comment,
                    Asset = assetId,
                    Amount = amount,
                    WalletId = sourceWalletId,
                    TransactionId = cashOutTransactionId
                });
            }
            catch (BalanceOperationException exception) when (exception.Code == 401)
            {
                _log.WarningWithDetails("No enough funds to cash out", new
                {
                    Comment = comment,
                    Asset = assetId,
                    Amount = amount,
                    WalletId = sourceWalletId
                });

                return SettlementError.NotEnoughFunds;
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, "An error occurred while cash out", new
                {
                    Comment = comment,
                    Asset = assetId,
                    Amount = amount,
                    WalletId = sourceWalletId
                });

                return SettlementError.Unknown;
            }

            try
            {
                string cashInTransactionId = await _lykkeExchangeService.CashInAsync(targetWalletId,
                    assetSettings.AssetId, amount, "settlement", comment);

                _log.InfoWithDetails("Cash in", new
                {
                    Comment = comment,
                    Asset = assetId,
                    Amount = amount,
                    WalletId = targetWalletId,
                    TransactionId = cashInTransactionId
                });
            }
            catch (Exception exception)
            {
                _log.ErrorWithDetails(exception, "An error occurred while cash in", new
                {
                    Comment = comment,
                    Asset = assetId,
                    Amount = amount,
                    WalletId = targetWalletId
                });

                return SettlementError.Unknown;
            }

            return SettlementError.None;
        }

        private async Task UpdateAssetsAsync(Settlement settlement, IReadOnlyCollection<AssetWeight> assetWeights)
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
                    IsDirect = settlement.IsDirect && assetSettlementAmount.Weight > AssetMinWeightToDirectTransfer,
                    IsExternal = assetHedgeSettings.Exchange != ExchangeNames.Lykke,
                    Status = AssetSettlementStatus.New,
                    ActualAmount = assetSettlementAmount.Amount,
                    ActualPrice = assetSettlementAmount.Price
                });
            }

            settlement.Assets = assetSettlements;
        }

        private async Task ValidateBalanceAsync(Settlement settlement)
        {
            IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                .Where(o=>o.Error == SettlementError.NotEnoughFunds);
            
            foreach (AssetSettlement assetSettlement in assetSettlements)
                assetSettlement.Error = SettlementError.None;

            AssetSettlement[] assetSettlementsDirect = settlement.Assets
                .Where(o => o.Error == SettlementError.None)
                .Where(o => o.IsDirect && !o.IsExternal)
                .ToArray();

            AssetSettlement[] assetSettlementsInUsd = settlement.Assets
                .Where(o => o.Error == SettlementError.None)
                .Where(o => !o.IsDirect)
                .ToArray();

            foreach (AssetSettlement assetSettlement in assetSettlementsDirect)
            {
                AssetSettings assetSettings =
                    await _instrumentService.GetAssetAsync(assetSettlement.AssetId, ExchangeNames.Lykke);
                
                Balance balance = _balanceService.GetByAssetId(ExchangeNames.Lykke, assetSettings.AssetId);

                if (balance.Amount - balance.Reserved < assetSettlement.Amount)
                    assetSettlement.Error = SettlementError.NotEnoughFunds;
            }

            decimal amountInUsd = assetSettlementsInUsd.Sum(o => o.Amount * o.Price);

            AssetSettings usdAssetSettings =
                await _instrumentService.GetAssetAsync("USD", ExchangeNames.Lykke);
            
            Balance usdBalance = _balanceService.GetByAssetId(ExchangeNames.Lykke, usdAssetSettings.AssetId);

            if (usdBalance.Amount - usdBalance.Reserved < amountInUsd)
            {
                foreach (AssetSettlement assetSettlement in assetSettlementsInUsd)
                    assetSettlement.Error = SettlementError.NotEnoughFunds;
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
