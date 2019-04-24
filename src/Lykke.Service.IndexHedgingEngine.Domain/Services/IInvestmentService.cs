using System.Collections.Generic;
using Lykke.Service.IndexHedgingEngine.Domain.Investments;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IInvestmentService
    {
        IReadOnlyCollection<AssetInvestment> GetAll();

        void Update(IReadOnlyCollection<AssetInvestment> assetInvestments);
    }
}
