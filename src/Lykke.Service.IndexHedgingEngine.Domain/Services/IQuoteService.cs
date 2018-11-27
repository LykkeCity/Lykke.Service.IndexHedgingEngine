using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IQuoteService
    {
        IReadOnlyCollection<Quote> GetAll();

        Quote GetByAssetPairId(string source, string assetPairId);

        decimal GetAvgMid(string assetPairId);
        
        void Update(Quote quote);
    }
}
