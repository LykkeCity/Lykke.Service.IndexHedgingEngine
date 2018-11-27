namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IRateService
    {
        Quote GetQuoteUsd(string assetId, string exchange);
        
        decimal GetRateInUsd(string assetId, string exchange);
        
        decimal ConvertToUsd(string assetId, string exchange, decimal amount);
        
        bool TryConvertToUsd(string assetId, string exchange, decimal amount, out decimal amountInUsd);
    }
}
