using System.Collections.Generic;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IInvestmentService
    {
        IReadOnlyCollection<AssetInvestment> GetAll();

        void Update(IReadOnlyCollection<AssetInvestment> assetInvestments);
    }
}
