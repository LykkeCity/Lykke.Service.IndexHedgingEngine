using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Repositories;

namespace Lykke.Service.IndexHedgingEngine.AzureRepositories.Balances
{
    public class FundingRepository : IFundingRepository
    {
        private readonly INoSQLTableStorage<FundingEntity> _storage;

        public FundingRepository(INoSQLTableStorage<FundingEntity> storage)
        {
            _storage = storage;
        }

        public async Task<Funding> GetAsync()
        {
            FundingEntity entity = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey());

            return Mapper.Map<Funding>(entity);
        }

        public async Task InsertOrReplaceAsync(Funding funding)
        {
            var entity = new FundingEntity(GetPartitionKey(), GetRowKey());

            Mapper.Map(funding, entity);

            await _storage.InsertOrReplaceAsync(entity);
        }

        private static string GetPartitionKey()
            => "Funding";

        private static string GetRowKey()
            => "Amount";
    }
}
