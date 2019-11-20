using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface ICrossAssetPairSettingsService
    {
        Task<IReadOnlyCollection<CrossAssetPairSettings>> GetAll();

        Task<IReadOnlyCollection<CrossAssetPairSettings>> FindCrossAssetPairsByIndex(string indexName, string shortIndexName);
    }
}
