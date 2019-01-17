using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settings;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class QuoteThresholdSettingsModelValidator : AbstractValidator<QuoteThresholdSettingsModel>
    {
        public QuoteThresholdSettingsModelValidator()
        {
            RuleFor(o => o.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Value should be greater than or equal to zero");
        }
    }
}
