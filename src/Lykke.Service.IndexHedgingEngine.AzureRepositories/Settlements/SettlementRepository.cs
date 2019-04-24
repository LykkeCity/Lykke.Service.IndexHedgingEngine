using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;
using Lykke.Service.IndexHedgingEngine.Domain.Settlements;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Settlements
{
    public class SettlementRepository : ISettlementRepository
    {
        private readonly SettlementAzureRepository _settlementAzureRepository;
        private readonly AssetSettlementAzureRepository _assetSettlementAzureRepository;

        public SettlementRepository(
            SettlementAzureRepository settlementAzureRepository,
            AssetSettlementAzureRepository assetSettlementAzureRepository)
        {
            _settlementAzureRepository = settlementAzureRepository;
            _assetSettlementAzureRepository = assetSettlementAzureRepository;
        }

        public async Task<IReadOnlyCollection<Settlement>> GetAllAsync()
        {
            IReadOnlyCollection<Settlement> settlements = await _settlementAzureRepository.GetAllAsync();

            IReadOnlyCollection<AssetSettlement> assetSettlements = await _assetSettlementAzureRepository.GetAllAsync();

            foreach (Settlement settlement in settlements)
                settlement.Assets = assetSettlements.Where(o => o.SettlementId == settlement.Id).ToArray();

            return settlements;
        }

        public async Task<IReadOnlyCollection<Settlement>> GetActiveAsync()
        {
            IReadOnlyCollection<Settlement> settlements = await GetAllAsync();

            var statuses = new[] {SettlementStatus.Approved, SettlementStatus.Reserved, SettlementStatus.Transferred};

            return settlements.Where(o => statuses.Contains(o.Status) && o.Error == SettlementError.None).ToArray();
        }
        
        public async Task<IReadOnlyCollection<Settlement>> GetByClientIdAsync(string clientId)
        {
            IReadOnlyCollection<Settlement> settlements = await GetAllAsync();

            return settlements.Where(o => o.ClientId == clientId).ToArray();
        }

        public async Task<Settlement> GetByIdAsync(string settlementId)
        {
            Settlement settlement = await _settlementAzureRepository.GetByIdAsync(settlementId);

            if (settlement == null)
                return null;

            settlement.Assets = await _assetSettlementAzureRepository.GetBySettlementIdAsync(settlementId);

            return settlement;
        }

        public Task InsertAsync(Settlement settlement)
        {
            return Task.WhenAll(
                _settlementAzureRepository.InsertAsync(settlement),
                _assetSettlementAzureRepository.InsertAsync(settlement.Assets));
        }

        public Task UpdateAsync(Settlement settlement)
        {
            return _settlementAzureRepository.UpdateAsync(settlement);
        }

        public Task UpdateAsync(AssetSettlement assetSettlement)
        {
            return _assetSettlementAzureRepository.UpdateAsync(assetSettlement);
        }

        public Task ReplaceAsync(Settlement settlement)
        {
            return Task.WhenAll(
                _settlementAzureRepository.UpdateAsync(settlement),
                _assetSettlementAzureRepository.ReplaceAsync(settlement.Id, settlement.Assets));
        }
    }
}
