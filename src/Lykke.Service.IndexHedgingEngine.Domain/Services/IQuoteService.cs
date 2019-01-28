using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IQuoteService
    {
        IReadOnlyCollection<Quote> GetAll();

        IReadOnlyCollection<string> GetExchanges();

        Quote GetByAssetPairId(string source, string assetPairId);

        Task UpdateAsync(Quote quote);
    }
}
