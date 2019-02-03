namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settlements
{
    public class SettlementProcessService
    {
        public async Task ReserveAsync()
        {
            IEnumerable<Settlement> settlements =
                await _settlementRepository.GetByStatusAsync(SettlementStatus.Approved);

            foreach (Settlement settlement in settlements.Where(o => !o.HasError()))
            {
                try
                {
                    IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                        .Where(o => !o.HasError() && o.Status == AssetSettlementStatus.New);

                    foreach (AssetSettlement assetSettlement in assetSettlements)
                    {
                        await ReserveFundsAsync(assetSettlement);

                        //if
                        // set error
                        //else
                        // set status

                        // update
                    }

                    if (settlement.Assets.All(o => o.Status == AssetSettlementStatus.Reserved))
                        await ReserveClientFundsAsync(settlement);

                    //if
                    // set error
                    //else
                    // set status

                    // update
                }
                catch (Exception exception)
                {
                    _log.WarningWithDetails("An error occurred while reserving funds for settlement", exception,
                        settlement);
                }
            }
        }

        public async Task ProcessAsync()
        {
            IEnumerable<Settlement> settlements =
                await _settlementRepository.GetByStatusAsync(SettlementStatus.Reserved);

            foreach (Settlement settlement in settlements.Where(o => !o.HasError()))
            {
                try
                {
                    IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                        .Where(o => !o.HasError() && o.Status == AssetSettlementStatus.Reserved);

                    foreach (AssetSettlement assetSettlement in assetSettlements)
                        await ProcessAssetSettlementAsync(assetSettlement);

                    if (settlement.Assets.All(o => o.Status == AssetSettlementStatus.Processed))
                        await ProcessSettlementAsync(settlement);
                }
                catch (Exception exception)
                {
                    _log.WarningWithDetails("An error occurred while closing positions of settlement", exception,
                        settlement);
                }
            }
        }

        public async Task TransferAsync()
        {
            IEnumerable<Settlement> settlements =
                await _settlementRepository.GetByStatusAsync(SettlementStatus.Processed);

            foreach (Settlement settlement in settlements.Where(o => !o.HasError()))
            {
                try
                {
                    IEnumerable<AssetSettlement> assetSettlements = settlement.Assets
                        .Where(o => !o.IsManual() && !o.HasError() && o.Status == AssetSettlementStatus.Processed);

                    foreach (AssetSettlement assetSettlement in assetSettlements)
                    {
                        await TransferReservedFundsAsync(assetSettlement, settlement.ClientId,
                            settlement.WalletId);
                    }

                    if (settlement.Assets.All(o => o.Status == AssetSettlementStatus.Transferred))
                        await TransferClientReservedFundsAsync(settlement);
                }
                catch (Exception exception)
                {
                    _log.WarningWithDetails("An error occurred while transferring settlement", exception, settlement);
                }
            }
        }

        public async Task CompleteAsync()
        {
            IEnumerable<Settlement> settlements =
                await _settlementRepository.GetByStatusAsync(SettlementStatus.Transferred);

            foreach (Settlement settlement in settlements.Where(o => !o.HasError()))
            {
                try
                {
                    if (settlement.Assets.All(o => !o.HasError() && o.Status == AssetSettlementStatus.Transferred))
                    {
                        settlement.Status = SettlementStatus.Completed;
                        await _settlementRepository.UpdateAsync(settlement);
                    }
                }
                catch (Exception exception)
                {
                    _log.WarningWithDetails("An error occurred while completing settlement", exception, settlement);
                }
            }
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

            if (settlement.Status != SettlementStatus.Processed)
                throw new InvalidOperationException("Can not execute asset.");

            assetSettlement.ActualAmount = actualAmount;
            assetSettlement.ActualPrice = actualPrice;
            assetSettlement.Status = AssetSettlementStatus.Transferred;

            await _settlementRepository.UpdateAsync(assetSettlement);

            // TODO: Cash out actual amount in USD from transit wallet.
            // TODO: Cash out remaining loss from main wallet

            _log.InfoWithDetails("Asset updated", new {assetSettlement, userId});
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

            if (settlement.Status != SettlementStatus.Reserved)
            {
                settlement.Status = SettlementStatus.Rejected;
                await _settlementRepository.UpdateAsync(settlement);
            }
            else
            {
                await ReleaseClientReservedFundsAsync(settlement);
            }

            _log.InfoWithDetails("Settlement rejected", new {settlement.Id, userId});
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

        private async Task ReserveFundsAsync(AssetSettlement assetSettlement)
        {
            try
            {
                if (assetSettlement.IsDirect)
                {
                    await _settlementTransferService.ReserveUsdFundsAsync(assetSettlement.AssetId,
                        assetSettlement.Price, assetSettlement.Amount, assetSettlement.SettlementId);
                }
                else
                {
                    await _settlementTransferService.ReserveFundsAsync(assetSettlement.AssetId, assetSettlement.Amount,
                        assetSettlement.SettlementId);
                }

                assetSettlement.Status = AssetSettlementStatus.Reserved;

                _log.InfoWithDetails("Funds reserved from main wallet", assetSettlement);
            }
            catch (NotEnoughFundsException)
            {
                assetSettlement.Error = SettlementError.NotEnoughFunds;

                _log.WarningWithDetails("Not enough funds to reserve from main wallet", assetSettlement);
            }
            catch (Exception exception)
            {
                assetSettlement.Error = SettlementError.Unknown;

                _log.ErrorWithDetails(exception, "An error occurred while reserving funds from main wallet",
                    assetSettlement);
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

        private async Task ProcessAssetSettlementAsync(AssetSettlement assetSettlement)
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
                        assetSettlement.Amount, assetSettlement.Price);
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

        private async Task ProcessSettlementAsync(Settlement settlement)
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

                _log.WarningWithDetails("Not enough reserved market maker funds to release",
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
    }
}
