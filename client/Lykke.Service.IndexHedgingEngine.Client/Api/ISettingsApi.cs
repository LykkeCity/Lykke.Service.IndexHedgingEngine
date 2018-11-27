using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settings;
using Refit;

namespace Lykke.Service.IndexHedgingEngine.Client.Api
{
    /// <summary>
    /// Provides methods for working with service settings.
    /// </summary>
    [PublicAPI]
    public interface ISettingsApi
    {
        /// <summary>
        /// Returns an account settings that used by service.
        /// </summary>
        /// <returns>The model that represents account settings.</returns>
        [Get("/api/Settings/account")]
        Task<AccountSettingsModel> GetAccountSettingsAsync();

        /// <summary>
        /// Returns a collection of exchanges.
        /// </summary>
        /// <returns>A collection of exchange settings.</returns>
        [Get("/api/Settings/exchanges")]
        Task<IReadOnlyCollection<ExchangeSettingsModel>> GetExchangesAsync();

        /// <summary>
        /// Returns the hedge algorithm settings.
        /// </summary>
        /// <returns>The model that represents hedge settings.</returns>
        [Get("/api/Settings/hedge")]
        Task<HedgeSettingsModel> GetHedgeSettingsAsync();

        /// <summary>
        /// Updates the hedge algorithm settings.
        /// </summary>
        /// <param name="model">The model that represents hedge settings.</param>
        [Put("/api/Settings/hedge")]
        Task UpdateHedgeSettingsAsync([Body] HedgeSettingsModel model);

        /// <summary>
        /// Returns the timers settings.
        /// </summary>
        /// <returns>The model that represents timers settings.</returns>
        [Get("/api/Settings/timers")]
        Task<TimersSettingsModel> GetTimersSettingsAsync();

        /// <summary>
        /// Updates the timers settings.
        /// </summary>
        /// <param name="model">The model that represents timers settings.</param>
        [Put("/api/Settings/timers")]
        Task UpdateTimersSettingsAsync([Body] TimersSettingsModel model);
    }
}
