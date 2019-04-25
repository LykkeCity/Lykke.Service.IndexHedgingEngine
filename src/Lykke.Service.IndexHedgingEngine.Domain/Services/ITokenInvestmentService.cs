using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ITokenInvestmentService
    {
        Task<IReadOnlyCollection<TokenInvestment>> GetAllAsync();

        Task<TokenInvestment> GetAsync(string indexAssetPairId);

        Task<Guid> AddAsync(CrossIndexSettings entity);

        Task UpdateAsync(CrossIndexSettings entity);

        Task DeleteAsync(string indexAssetPairId);
    }
}
