using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Balances
{
    public class TokenRepository : ITokenRepository
    {
        private readonly INoSQLTableStorage<TokenEntity> _storage;

        public TokenRepository(INoSQLTableStorage<TokenEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyCollection<Token>> GetAllAsync()
        {
            IEnumerable<TokenEntity> entities = await _storage.GetDataAsync(GetPartitionKey());

            return Mapper.Map<Token[]>(entities);
        }

        public async Task InsertOrReplaceAsync(Token token)
        {
            var entity = new TokenEntity(GetPartitionKey(), GetRowKey(token.AssetId));

            Mapper.Map(token, entity);

            await _storage.InsertOrReplaceAsync(entity);
        }

        private static string GetPartitionKey()
            => "Token";

        private static string GetRowKey(string assetId)
            => assetId.ToUpper();
    }
}
