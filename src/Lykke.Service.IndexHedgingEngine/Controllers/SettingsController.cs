using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settings;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Constants;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.IndexHedgingEngine.Controllers
{
    [Route("/api/[controller]")]
    public class SettingsController : Controller, ISettingsApi
    {
        private readonly ISettingsService _settingsService;
        private readonly IHedgeSettingsService _hedgeSettingsService;
        private readonly ITimersSettingsService _timersSettingsService;
        private readonly IQuoteService _quoteService;
        private readonly IQuoteThresholdSettingsService _quoteThresholdSettingsService;

        public SettingsController(
            ISettingsService settingsService,
            IHedgeSettingsService hedgeSettingsService,
            ITimersSettingsService timersSettingsService,
            IQuoteService quoteService,
            IQuoteThresholdSettingsService quoteThresholdSettingsService)
        {
            _settingsService = settingsService;
            _hedgeSettingsService = hedgeSettingsService;
            _timersSettingsService = timersSettingsService;
            _quoteService = quoteService;
            _quoteThresholdSettingsService = quoteThresholdSettingsService;
        }

        /// <inheritdoc/>
        /// <response code="200">The model that represents account settings.</response>
        [HttpGet("account")]
        [ProducesResponseType(typeof(AccountSettingsModel), (int) HttpStatusCode.OK)]
        public Task<AccountSettingsModel> GetAccountSettingsAsync()
        {
            string walletId = _settingsService.GetWalletId();

            return Task.FromResult(new AccountSettingsModel {WalletId = walletId});
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of exchange settings.</response>
        [HttpGet("exchanges")]
        [ProducesResponseType(typeof(ExchangeSettingsModel), (int) HttpStatusCode.OK)]
        public Task<IReadOnlyCollection<ExchangeSettingsModel>> GetExchangesAsync()
        {
            IReadOnlyCollection<ExchangeSettings> exchangeSettings = _settingsService.GetExchanges();

            IEnumerable<ExchangeSettings> exchanges = _quoteService.GetExchanges()
                .Where(o => !o.Equals(ExchangeNames.Lykke))
                .Select(o =>
                    new ExchangeSettings
                    {
                        Name = o,
                        Fee = 0,
                        HasApi = false
                    });

            var model = Mapper.Map<ExchangeSettingsModel[]>(exchangeSettings.Union(exchanges));

            return Task.FromResult<IReadOnlyCollection<ExchangeSettingsModel>>(model);
        }

        /// <inheritdoc/>
        /// <response code="200">The model that represents hedge settings.</response>
        [HttpGet("hedge")]
        [ProducesResponseType(typeof(HedgeSettingsModel), (int) HttpStatusCode.OK)]
        public async Task<HedgeSettingsModel> GetHedgeSettingsAsync()
        {
            HedgeSettings hedgeSettings = await _hedgeSettingsService.GetAsync();

            return Mapper.Map<HedgeSettingsModel>(hedgeSettings);
        }

        /// <inheritdoc/>
        /// <response code="204">The hedge settings successfully updated.</response>
        [HttpPut("hedge")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task UpdateHedgeSettingsAsync([FromBody] HedgeSettingsModel model)
        {
            var hedgeSettings = Mapper.Map<HedgeSettings>(model);

            await _hedgeSettingsService.UpdateAsync(hedgeSettings);
        }

        /// <inheritdoc/>
        /// <response code="200">The model that represents timers settings.</response>
        [HttpGet("timers")]
        [ProducesResponseType(typeof(TimersSettingsModel), (int) HttpStatusCode.OK)]
        public async Task<TimersSettingsModel> GetTimersSettingsAsync()
        {
            TimersSettings timersSettings = await _timersSettingsService.GetAsync();

            return Mapper.Map<TimersSettingsModel>(timersSettings);
        }

        /// <inheritdoc/>
        /// <response code="204">The timers settings successfully updated.</response>
        [HttpPut("timers")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task UpdateTimersSettingsAsync([FromBody] TimersSettingsModel model)
        {
            var timersSettings = Mapper.Map<TimersSettings>(model);

            await _timersSettingsService.UpdateAsync(timersSettings);
        }

        /// <inheritdoc />
        /// <response code="200">The settings of quote threshold.</response>
        [HttpGet("quotes/threshold")]
        [ProducesResponseType(typeof(QuoteThresholdSettingsModel), (int) HttpStatusCode.OK)]
        public async Task<QuoteThresholdSettingsModel> GetQuoteThresholdSettingsAsync()
        {
            QuoteThresholdSettings quoteThresholdSettings = await _quoteThresholdSettingsService.GetAsync();

            return Mapper.Map<QuoteThresholdSettingsModel>(quoteThresholdSettings);
        }

        /// <inheritdoc />
        /// <response code="204">The settings of quote threshold successfully saved.</response>
        [HttpPost("quotes/threshold")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task SaveQuoteThresholdSettingsAsync([FromBody] QuoteThresholdSettingsModel model)
        {
            var quoteThresholdSettings = Mapper.Map<QuoteThresholdSettings>(model);

            await _quoteThresholdSettingsService.UpdateAsync(quoteThresholdSettings);
        }
    }
}
