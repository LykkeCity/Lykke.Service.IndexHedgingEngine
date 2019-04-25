using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface ITokenInvestmentRepository
    {
        Task<IReadOnlyList<TokenInvestment>> GetAllAsync();

        Task InsertAsync(TokenInvestment entity);

        Task UpdateAsync(TokenInvestment entity);

        Task DeleteAsync(string indexAssetPairId);
    }
}
