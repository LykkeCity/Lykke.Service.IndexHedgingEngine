using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.IndexHedgingEngine.Client.Api;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settings;
using Lykke.Service.IndexHedgingEngine.Domain;
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

        public SettingsController(
            ISettingsService settingsService,
            IHedgeSettingsService hedgeSettingsService,
            ITimersSettingsService timersSettingsService)
        {
            _settingsService = settingsService;
            _hedgeSettingsService = hedgeSettingsService;
            _timersSettingsService = timersSettingsService;
        }

        /// <inheritdoc/>
        /// <response code="200">The model that represents account settings.</response>
        [HttpGet("account")]
        [ProducesResponseType(typeof(AccountSettingsModel), (int) HttpStatusCode.OK)]
        public async Task<AccountSettingsModel> GetAccountSettingsAsync()
        {
            string walletId = await _settingsService.GetWalletIdAsync();

            return new AccountSettingsModel {WalletId = walletId};
        }

        /// <inheritdoc/>
        /// <response code="200">A collection of exchange settings.</response>
        [HttpGet("exchanges")]
        [ProducesResponseType(typeof(AccountSettingsModel), (int) HttpStatusCode.OK)]
        public async Task<IReadOnlyCollection<ExchangeSettingsModel>> GetExchangesAsync()
        {
            IReadOnlyCollection<ExchangeSettings> exchangeSettings = await _settingsService.GetExchangesAsync();

            return Mapper.Map<ExchangeSettingsModel[]>(exchangeSettings);
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
    }
}
