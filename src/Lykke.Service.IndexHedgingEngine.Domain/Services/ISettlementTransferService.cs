using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ISettlementTransferService
    {
        Task ReserveFundsAsync(string asset, decimal amount, string clientId, string settlementId);

        Task ReserveClientFundsAsync(string walletId, string asset, decimal amount, string clientId,
            string settlementId);

        Task<string> TransferReservedFundsAsync(string walletId, string asset, decimal amount,
            string clientId, string settlementId);

        Task<string> TransferClientReservedFundsAsync(string asset, decimal amount, string clientId,
            string settlementId);

        Task ReleaseReservedFundsAsync(string asset, decimal amount, string clientId, string settlementId);

        Task ReleaseClientReservedFundsAsync(string walletId, string asset, decimal amount, string clientId,
            string settlementId);
    }
}
