using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.IndexHedgingEngine.Domain;
using Lykke.Service.IndexHedgingEngine.Domain.Handlers;
using Lykke.Service.IndexHedgingEngine.Domain.Services;
using Lykke.Service.IndexHedgingEngine.Domain.Settings;

namespace Lykke.Service.IndexHedgingEngine.DomainServices.Timers
{
    [UsedImplicitly]
    public class SettlementsTimer : Timer
    {
        private readonly ISettlementHandler _settlementHandler;
        private readonly ITimersSettingsService _timersSettingsService;

        public SettlementsTimer(
            ISettlementHandler settlementHandler,
            ITimersSettingsService timersSettingsService,
            ILogFactory logFactory)
        {
            _settlementHandler = settlementHandler;
            _timersSettingsService = timersSettingsService;
            Log = logFactory.CreateLog(this);
        }

        protected override async Task<TimeSpan> GetDelayAsync()
        {
            TimersSettings timersSettings = await _timersSettingsService.GetAsync();

            return timersSettings.Settlements == TimeSpan.Zero
                ? TimeSpan.FromMinutes(5)
                : timersSettings.Settlements;
        }

        protected override Task OnExecuteAsync(CancellationToken cancellation)
        {
            return _settlementHandler.ExecuteAsync();
        }
    }
}
