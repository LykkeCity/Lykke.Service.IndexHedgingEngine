using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Balances
{
    public class TokenInvestmentRepository : ITokenInvestmentRepository
    {
        private readonly INoSQLTableStorage<TokenInvestmentEntity> _storage;

        public TokenInvestmentRepository(INoSQLTableStorage<TokenInvestmentEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<TokenInvestment>> GetAllAsync()
        {
            IList<TokenInvestmentEntity> entities = await _storage.GetDataAsync();

            return Mapper.Map<TokenInvestment[]>(entities);
        }

        public async Task InsertAsync(TokenInvestment entity)
        {
            var newEntity = new TokenInvestmentEntity(
                GetPartitionKey(),
                GetRowKey(entity.AssetPairId));

            Mapper.Map(entity, newEntity);

            await _storage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(TokenInvestment entity)
        {
            return _storage.MergeAsync(
                GetPartitionKey(),
                GetRowKey(entity.AssetPairId),
                x =>
                {
                    Mapper.Map(entity, x);
                    return x;
                });
        }

        public Task DeleteAsync(string indexAssetPairId)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(indexAssetPairId));
        }

        private static string GetPartitionKey()
            => "TokenInvestment";

        private static string GetRowKey(string indexAssetPairId)
            => $"{indexAssetPairId}";
    }
}
