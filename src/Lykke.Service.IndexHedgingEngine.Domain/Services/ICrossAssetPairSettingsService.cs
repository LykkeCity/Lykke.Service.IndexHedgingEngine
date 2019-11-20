using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ICrossAssetPairSettingsService
    {
        Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAllAsync();

        Task<IReadOnlyCollection<CrossAssetPairSettings>> FindCrossAssetPairsByIndexAsync(string indexName, string shortIndexName);
    }
}
