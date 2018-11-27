using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Repositories
{
    public interface ITokenRepository
    {
        Task<IReadOnlyCollection<Token>> GetAllAsync();

        Task InsertOrReplaceAsync(Token token);
    }
}
