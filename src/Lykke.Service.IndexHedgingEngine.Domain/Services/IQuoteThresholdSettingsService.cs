﻿using System.Threading.Tasks;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.Domain.Services
{
    public interface IQuoteThresholdSettingsService
    {
        Task<QuoteThresholdSettings> GetAsync();

        Task UpdateAsync(QuoteThresholdSettings quoteThresholdSettings);
    }
}
