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
using Lykke.Service.IndexHedgingEngine.Domain.Settings;
using Lykke.Service.IndexHedgingEngine.Domain.Settlements;
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
        private readonly ISettlementTransferService _settlementTransferService;
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
            ISettlementTransferService settlementTransferService,
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
            _settlementTransferService = settlementTransferService;
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
            IEnumerable<Settlement> settlements = await _settlementRepository.GetActiveAsync();

            foreach (Settlement settlement in settlements)
            {
                try
                {
                    await ReserveAsync(settlement);

                    await TransferAsync(settlement);

                    await CompleteAsync(settlement);
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

            Settlement settlement = await CreateSettlementAsync(indexName, amount, comment, walletId, clientId, userId,
                isDirect, indexPrice);

            await ValidateBalanceAsync(settlement);

            await _settlementRepository.InsertAsync(settlement);

            _log.InfoWithDetails("Settlement created", settlement);
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

            await CalculateAssetSettlementsAsync(settlement, indexPrice.Weights);

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
                .Any(o => o.Status == AssetSettlementStatus.Transferred || o.Status == AssetSettlementStatus.Completed);

            if (!allowedStatuses.Contains(settlement.Status) || hasTransferredAssets)
                throw new InvalidOperationException("Settlement can not be rejected");

            IEnumerable<AssetSettlement> reservedAssets = settlement.Assets
                .Where(o => o.Status == AssetSettlementStatus.Reserved);

            foreach (AssetSettlement assetSettlement in reservedAssets)
                await ReleaseReservedFundsAsync(assetSettlement, settlement.ClientId);

            await ReleaseClientReservedFundsAsync(settlement);

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

            if (settlement.Status != SettlementStatus.Approved &&
                settlement.Status != SettlementStatus.Reserved)
            {
                throw new InvalidOperationException("Can not retry.");
            }

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

            if (settlement.Status != SettlementStatus.Approved &&
                settlement.Status != SettlementStatus.Reserved ||
                assetSettlement.Status != AssetSettlementStatus.New &&
                assetSettlement.Status != AssetSettlementStatus.Reserved)
            {
                throw new InvalidOperationException("Can not retry asset.");
            }
            
            assetSettlement.Error = SettlementError.None;
            
            await _settlementRepository.UpdateAsync(assetSettlement);

            _log.InfoWithDetails("Asset settlement retry", new {assetSettlement, userId});
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

            var allowedStatuses = new[] {SettlementStatus.Reserved, SettlementStatus.Transferred};
            
            if(!allowedStatuses.Contains(settlement.Status) || assetSettlement.Status != AssetSettlementStatus.New)
                throw new InvalidOperationException("Can not execute asset.");
            
            assetSettlement.ActualAmount = actualAmount;
            assetSettlement.ActualPrice = actualPrice;
            assetSettlement.Status = AssetSettlementStatus.Transferred;

            await _settlementRepository.UpdateAsync(assetSettlement);

            _log.InfoWithDetails("Asset updated", new {assetSettlement, userId});
        }

        private async Task<Settlement> CreateSettlementAsync(string indexName, decimal amount, string comment,
            string walletId, string clientId, string userId, bool isDirect, IndexPrice indexPrice)
        {
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

            await CalculateAssetSettlementsAsync(settlement, indexPrice.Weights);

            return settlement;
        }

        private async Task CalculateAssetSettlementsAsync(Settlement settlement, IEnumerable<AssetWeight> assetWeights)
        {
            decimal amountInUsd = settlement.Amount * settlement.Price;
            
            var assetSettlements = new List<AssetSettlement>();

            foreach (AssetWeight assetWeight in assetWeights)
            {
                AssetHedgeSettings assetHedgeSettings =
                    await _assetHedgeSettingsService.EnsureAsync(assetWeight.AssetId);

                Quote quote = _quoteService.GetByAssetPairId(assetHedgeSettings.Exchange,
                    assetHedgeSettings.AssetPairId);

                decimal amount = 0;
                decimal price = 0;

                if (quote != null)
                {
                    amount = amountInUsd * assetWeight.Weight / quote.Mid;
                    price = quote.Mid;
                }

                assetSettlements.Add(new AssetSettlement
                {
                    AssetId = assetWeight.AssetId,
                    SettlementId = settlement.Id,
                    Amount = amount,
                    Price = price,
                    Fee = decimal.Zero,
                    Weight = assetWeight.Weight,
                    IsDirect = settlement.IsDirect && assetWeight.Weight > AssetMinWeightToDirectTransfer,
                    IsExternal = assetHedgeSettings.Exchange != ExchangeNames.Lykke,
                    Status = AssetSettlementStatus.New,
                    ActualAmount = amount,
                    ActualPrice = price,
                    Error = quote == null ? SettlementError.NoQuote : SettlementError.None
                });
            }
            
            settlement.Assets = assetSettlements;
        }

        private async Task ReserveAsync(Settlement settlement)
        {
            if (settlement.Status != SettlementStatus.Approved)
                return;
            
            IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                .Where(o => !o.IsManual())
                .Where(o => o.Status == AssetSettlementStatus.New)
                .Where(o => o.Error == SettlementError.None);

            foreach (AssetSettlement assetSettlement in assetSettlements)
                await ReserveFundsAsync(assetSettlement, settlement.ClientId);

            bool reserved = settlement.Assets
                .Where(o => !o.IsManual())
                .All(o => o.Status == AssetSettlementStatus.Reserved);

            if (reserved)
                await ReserveClientFundsAsync(settlement);
        }

        private async Task ReserveFundsAsync(AssetSettlement assetSettlement, string clientId)
        {
            await _settlementRepository.UpdateAsync(assetSettlement);

            string assetId = assetSettlement.IsDirect
                ? assetSettlement.AssetId
                : "USD";

            decimal amount = assetSettlement.IsDirect
                ? assetSettlement.Amount
                : assetSettlement.Amount * assetSettlement.Price;

            try
            {
                await _settlementTransferService.ReserveFundsAsync(assetId, amount, clientId,
                    assetSettlement.SettlementId);

                assetSettlement.Status = AssetSettlementStatus.Reserved;

                _log.InfoWithDetails("Funds reserved from main wallet",
                    new {assetSettlement.SettlementId, assetId, amount, clientId});
            }
            catch (NotEnoughFundsException)
            {
                assetSettlement.Error = SettlementError.NotEnoughFunds;

                _log.WarningWithDetails("Not enough funds to reserve from main wallet",
                    new {assetSettlement.SettlementId, assetId, amount, clientId});
            }
            catch (Exception exception)
            {
                assetSettlement.Error = SettlementError.Unknown;

                _log.ErrorWithDetails(exception, "An error occurred while reserving funds from main wallet",
                    new {assetSettlement.SettlementId, assetId, amount, clientId});
            }

            await _settlementRepository.UpdateAsync(assetSettlement);
        }

        private async Task ReserveClientFundsAsync(Settlement settlement)
        {
            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(settlement.IndexName);

            // TODO: Use internal asset
            AssetSettings assetSettings = (await _instrumentService.GetAssetsAsync())
                .Single(o => o.Exchange == ExchangeNames.Lykke && o.AssetId == indexSettings.AssetId);

            try
            {
                await _settlementTransferService.ReserveClientFundsAsync(settlement.WalletId, assetSettings.Asset,
                    settlement.Amount, settlement.ClientId, settlement.Id);

                settlement.Status = SettlementStatus.Reserved;

                _log.InfoWithDetails("Funds reserved from client wallet",
                    new {SettlementId = settlement.Id, indexSettings.AssetId, settlement.Amount, settlement.WalletId});
            }
            catch (NotEnoughFundsException)
            {
                settlement.Error = SettlementError.NotEnoughFunds;

                _log.WarningWithDetails("Not enough funds to reserve from client wallet",
                    new {SettlementId = settlement.Id, indexSettings.AssetId, settlement.Amount, settlement.WalletId});
            }
            catch (Exception exception)
            {
                settlement.Error = SettlementError.Unknown;

                _log.ErrorWithDetails(exception, "An error occurred while reserving funds from client wallet",
                    new {SettlementId = settlement.Id, indexSettings.AssetId, settlement.Amount, settlement.WalletId});
            }

            await _settlementRepository.UpdateAsync(settlement);
        }

        private async Task TransferAsync(Settlement settlement)
        {
            if (settlement.Status != SettlementStatus.Reserved)
                return;

            IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                .Where(o => !o.IsManual())
                .Where(o => o.Status == AssetSettlementStatus.Reserved)
                .Where(o => o.Error == SettlementError.None);

            foreach (AssetSettlement assetSettlement in assetSettlements)
            {
                await TransferReservedFundsAsync(assetSettlement, settlement.ClientId,
                    settlement.WalletId);
            }

            bool transferred = settlement.Assets
                .Where(o => !o.IsManual())
                .All(o => o.Status == AssetSettlementStatus.Transferred);

            if (transferred)
                await TransferClientReservedFundsAsync(settlement);
        }

        private async Task TransferReservedFundsAsync(AssetSettlement assetSettlement, string clientId,
            string walletId)
        {
            string assetId = assetSettlement.IsDirect
                ? assetSettlement.AssetId
                : "USD";

            decimal amount = assetSettlement.IsDirect
                ? assetSettlement.Amount
                : assetSettlement.Amount * assetSettlement.Price;

            try
            {
                string transactionId = await _settlementTransferService.TransferReservedFundsAsync(walletId,
                    assetId, amount, clientId, assetSettlement.SettlementId);

                assetSettlement.TransactionId = transactionId;
                assetSettlement.Status = AssetSettlementStatus.Transferred;

                _log.InfoWithDetails("Reserved funds transferred to client wallet",
                    new {assetSettlement.SettlementId, assetId, amount, clientId, walletId, transactionId});
            }
            catch (NotEnoughFundsException)
            {
                assetSettlement.Error = SettlementError.NotEnoughFunds;

                _log.WarningWithDetails("Not enough reserved funds to transfer to client wallet",
                    new {assetSettlement.SettlementId, assetId, amount, clientId, walletId});
            }
            catch (Exception exception)
            {
                assetSettlement.Error = SettlementError.Unknown;

                _log.ErrorWithDetails(exception, "An error occurred while transferring reserved funds to client wallet",
                    new {assetSettlement.SettlementId, assetId, amount, clientId, walletId});
            }

            await _settlementRepository.UpdateAsync(assetSettlement);
        }

        private async Task TransferClientReservedFundsAsync(Settlement settlement)
        {
            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(settlement.IndexName);

            // TODO: Use internal asset
            AssetSettings assetSettings = (await _instrumentService.GetAssetsAsync())
                .Single(o => o.Exchange == ExchangeNames.Lykke && o.AssetId == indexSettings.AssetId);

            try
            {
                string transactionId = await _settlementTransferService.TransferClientReservedFundsAsync(
                    assetSettings.Asset, settlement.Amount, settlement.ClientId, settlement.Id);

                settlement.TransactionId = transactionId;
                settlement.Status = SettlementStatus.Transferred;

                _log.InfoWithDetails("Reserved funds transferred to main wallet",
                    new
                    {
                        SettlementId = settlement.Id,
                        indexSettings.AssetId,
                        settlement.Amount,
                        settlement.WalletId,
                        transactionId
                    });
            }
            catch (NotEnoughFundsException)
            {
                settlement.Error = SettlementError.NotEnoughFunds;

                _log.WarningWithDetails("Not enough reserved funds to transfer to main wallet",
                    new {SettlementId = settlement.Id, indexSettings.AssetId, settlement.Amount, settlement.WalletId});
            }
            catch (Exception exception)
            {
                settlement.Error = SettlementError.Unknown;

                _log.ErrorWithDetails(exception, "An error occurred while transferring reserved funds to main wallet",
                    new {SettlementId = settlement.Id, indexSettings.AssetId, settlement.Amount, settlement.WalletId});
            }

            await _settlementRepository.UpdateAsync(settlement);
        }

        private async Task CompleteAsync(Settlement settlement)
        {
            if (settlement.Status != SettlementStatus.Transferred)
                return;

            IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                .Where(o => o.Status == AssetSettlementStatus.Transferred)
                .Where(o => o.Error == SettlementError.None);

            foreach (AssetSettlement assetSettlement in assetSettlements)
                await CompleteAssetSettlementAsync(assetSettlement);

            bool completed = settlement.Assets.All(o => o.Status == AssetSettlementStatus.Completed);

            if (completed)
                await CompleteSettlementAsync(settlement);
        }

        private async Task CompleteAssetSettlementAsync(AssetSettlement assetSettlement)
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

        private async Task CompleteSettlementAsync(Settlement settlement)
        {
            try
            {
                IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(settlement.IndexName);

                await _tokenService.CloseAsync(indexSettings.AssetId, settlement.Amount, settlement.Price);

                settlement.Status = SettlementStatus.Completed;
            }
            catch (Exception exception)
            {
                settlement.Error = SettlementError.Unknown;
                
                _log.ErrorWithDetails(exception, "An error occurred while completing settlement", settlement);
            }

            await _settlementRepository.UpdateAsync(settlement);
        }

        private async Task ReleaseReservedFundsAsync(AssetSettlement assetSettlement, string clientId)
        {
            string assetId = assetSettlement.IsDirect
                ? assetSettlement.AssetId
                : "USD";

            decimal amount = assetSettlement.IsDirect
                ? assetSettlement.Amount
                : assetSettlement.Amount * assetSettlement.Price;

            try
            {
                await _settlementTransferService.ReleaseReservedFundsAsync(assetId, amount, clientId,
                    assetSettlement.SettlementId);

                assetSettlement.Status = AssetSettlementStatus.Cancelled;

                _log.InfoWithDetails("Reserved market maker funds released",
                    new {assetSettlement.SettlementId, assetId, amount, clientId});
            }
            catch (NotEnoughFundsException)
            {
                assetSettlement.Error = SettlementError.NotEnoughFunds;

                _log.WarningWithDetails("Not enough reserved market maker funds to released",
                    new {assetSettlement.SettlementId, assetId, amount, clientId});
            }
            catch (Exception exception)
            {
                assetSettlement.Error = SettlementError.Unknown;

                _log.ErrorWithDetails(exception, "An error occurred while releasing reserved market maker funds",
                    new {assetSettlement.SettlementId, assetId, amount, clientId});
            }

            await _settlementRepository.UpdateAsync(assetSettlement);
        }
        
        private async Task ReleaseClientReservedFundsAsync(Settlement settlement)
        {
            if (settlement.Status != SettlementStatus.Reserved)
            {
                settlement.Status = SettlementStatus.Rejected;
                await _settlementRepository.UpdateAsync(settlement);
                return;
            }

            IndexSettings indexSettings = await _indexSettingsService.GetByIndexAsync(settlement.IndexName);

            // TODO: Use internal asset
            AssetSettings assetSettings = (await _instrumentService.GetAssetsAsync())
                .Single(o => o.Exchange == ExchangeNames.Lykke && o.AssetId == indexSettings.AssetId);

            try
            {
                await _settlementTransferService.ReleaseClientReservedFundsAsync(settlement.WalletId,
                    assetSettings.Asset, settlement.Amount, settlement.ClientId, settlement.Id);

                settlement.Status = SettlementStatus.Rejected;

                _log.InfoWithDetails("Reserved client funds released",
                    new {SettlementId = settlement.Id, indexSettings.AssetId, settlement.Amount, settlement.WalletId});
            }
            catch (NotEnoughFundsException)
            {
                settlement.Error = SettlementError.NotEnoughFunds;

                _log.WarningWithDetails("Not enough reserved client funds to release",
                    new {SettlementId = settlement.Id, indexSettings.AssetId, settlement.Amount, settlement.WalletId});
            }
            catch (Exception exception)
            {
                settlement.Error = SettlementError.Unknown;

                _log.ErrorWithDetails(exception, "An error occurred while releasing reserved client funds",
                    new {SettlementId = settlement.Id, indexSettings.AssetId, settlement.Amount, settlement.WalletId});
            }

            await _settlementRepository.UpdateAsync(settlement);
        }

        private async Task ValidateBalanceAsync(Settlement settlement)
        {
            IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                .Where(o => o.Error == SettlementError.NotEnoughFunds);

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
    }
}
