using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IQuoteService
    {
        IReadOnlyCollection<Quote> GetAll();

        IReadOnlyCollection<string> GetExchanges();

        Quote GetByAssetPairId(string assetPairId, string source = "lykke");

        Task UpdateAsync(Quote quote);
    }
}
