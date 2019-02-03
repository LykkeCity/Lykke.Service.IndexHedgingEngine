using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.DomainServices.Extensions;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Settlements
{
    public class SettlementTransferService : ISettlementTransferService
    {
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly IInstrumentService _instrumentService;
        private readonly ISettingsService _settingsService;
        private readonly ILog _log;

        public SettlementTransferService(
            ILykkeExchangeService lykkeExchangeService,
            IInstrumentService instrumentService,
            ISettingsService settingsService,
            ILogFactory logFactory)
        {
            _lykkeExchangeService = lykkeExchangeService;
            _instrumentService = instrumentService;
            _settingsService = settingsService;
            _log = logFactory.CreateLog(this);
        }

        public async Task ReserveFundsAsync(string asset, decimal amount, decimal price, bool isDirect,
            string settlementId)
        {
            
        }

        public async Task ReserveFundsAsync(string asset, decimal amount, string settlementId)
        {
            string comment = $"Reserve market maker funds. SettlementId: {settlementId}";

            string mainWalletId = _settingsService.GetWalletId();

            string transitWalletId = _settingsService.GetTransitWalletId();

            await CashOutAsync(asset, mainWalletId, amount, comment);

            await CashInAsync(asset, transitWalletId, amount, comment);
        }

        public async Task ReserveClientFundsAsync(string walletId, string asset, decimal amount, string clientId,
            string settlementId)
        {
            string comment = $"Reserve client funds. ClientId: {clientId}; SettlementId: {settlementId}";

            string transitWalletId = _settingsService.GetTransitWalletId();

            await CashOutAsync(asset, walletId, amount, comment);

            await CashInAsync(asset, transitWalletId, amount, comment);
        }

        public async Task<string> TransferReservedFundsAsync(string walletId, string asset, decimal amount,
            string clientId, string settlementId)
        {
            string comment = $"Transfer reserved funds to client. ClientId: {clientId}; SettlementId: {settlementId}";

            string transitWalletId = _settingsService.GetTransitWalletId();

            await CashOutAsync(asset, transitWalletId, amount, comment);

            return await CashInAsync(asset, walletId, amount, comment);
        }

        public async Task<string> TransferClientReservedFundsAsync(string asset, decimal amount, string clientId,
            string settlementId)
        {
            string comment =
                $"Transfer reserved funds to market maker. ClientId: {clientId}; SettlementId: {settlementId}";

            string mainWalletId = _settingsService.GetWalletId();

            string transitWalletId = _settingsService.GetTransitWalletId();

            await CashOutAsync(asset, transitWalletId, amount, comment);

            return await CashInAsync(asset, mainWalletId, amount, comment);
        }

        public async Task ReleaseReservedFundsAsync(string asset, decimal amount, string clientId, string settlementId)
        {
            string comment = $"Release reserved market maker funds. ClientId: {clientId}; SettlementId: {settlementId}";

            string mainWalletId = _settingsService.GetWalletId();

            string transitWalletId = _settingsService.GetTransitWalletId();

            await CashOutAsync(asset, transitWalletId, amount, comment);

            await CashInAsync(asset, mainWalletId, amount, comment);
        }

        public async Task ReleaseClientReservedFundsAsync(string walletId, string asset, decimal amount,
            string clientId, string settlementId)
        {
            string comment = $"Release reserved client funds. ClientId: {clientId}; SettlementId: {settlementId}";

            string transitWalletId = _settingsService.GetTransitWalletId();

            await CashOutAsync(asset, transitWalletId, amount, comment);

            await CashInAsync(asset, walletId, amount, comment);
        }

        private async Task<string> CashOutAsync(string asset, string walletId, decimal amount, string comment)
        {
            string assetId = await GetLykkeAssetIdAsync(asset);

            string transactionId = await _lykkeExchangeService.CashOutAsync(walletId, assetId, amount, "empty",
                comment);

            _log.InfoWithDetails("Cash out", new
            {
                Comment = comment,
                Asset = assetId,
                Amount = amount,
                WalletId = walletId,
                TransactionId = transactionId
            });

            return transactionId;
        }

        private async Task<string> CashInAsync(string asset, string walletId, decimal amount, string comment)
        {
            string assetId = await GetLykkeAssetIdAsync(asset);

            string transactionId = await _lykkeExchangeService.CashInAsync(walletId, assetId, amount, "empty", comment);

            _log.InfoWithDetails("Cash in", new
            {
                Comment = comment,
                Asset = assetId,
                Amount = amount,
                WalletId = walletId,
                TransactionId = transactionId
            });

            return transactionId;
        }

        // TODO: Move to LykkeExchangeService
        private async Task<string> GetLykkeAssetIdAsync(string asset)
        {
            AssetSettings assetSettings = await _instrumentService.GetAssetAsync(asset, ExchangeNames.Lykke);

            if (assetSettings == null)
            {
                _log.WarningWithDetails("No asset setting", asset);
                throw new InvalidOperationException("No asset setting");
            }

            return assetSettings.AssetId;
        }
    }
}
