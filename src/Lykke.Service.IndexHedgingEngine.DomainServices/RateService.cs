using System;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Services;

namespace Lykke.Service.IndexHedgingEngine.DomainServices
{
    public class RateService : IRateService
    {
        private readonly IQuoteService _quoteService;

        public RateService(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        public Quote GetQuoteUsd(string assetId, string exchange)
        {
            return _quoteService.GetByAssetPairId($"{assetId}USD", exchange);
        }

        public decimal GetRateInUsd(string assetId, string exchange)
        {
            Quote quote = GetQuoteUsd(assetId, exchange);

            if (quote == null)
                throw new InvalidOperationException("No quote");

            return quote.Mid;
        }

        public decimal ConvertToUsd(string assetId, string exchange, decimal amount)
        {
            return amount * GetRateInUsd(assetId, exchange);
        }

        public bool TryConvertToUsd(string assetId, string exchange, decimal amount, out decimal amountInUsd)
        {
            amountInUsd = decimal.Zero;

            try
            {
                amountInUsd = ConvertToUsd(assetId, exchange, amount);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
