using System;
using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settings;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class TimersSettingsModelValidator : AbstractValidator<TimersSettingsModel>
    {
        public TimersSettingsModelValidator()
        {
            RuleFor(o => o.LykkeBalances)
                .GreaterThanOrEqualTo(TimeSpan.FromSeconds(1))
                .WithMessage("Lykke balances timer interval should be greater than or equal to one second.");

            RuleFor(o => o.ExternalBalances)
                .GreaterThanOrEqualTo(TimeSpan.FromSeconds(1))
                .WithMessage("External balances timer interval should be greater than or equal to one second.");
        }
    }
}
