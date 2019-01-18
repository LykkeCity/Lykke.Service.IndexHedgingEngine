using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settings;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class HedgeSettingsModelValidator : AbstractValidator<HedgeSettingsModel>
    {
        public HedgeSettingsModelValidator()
        {
            RuleFor(o => o.ThresholdDown)
                .GreaterThan(0)
                .WithMessage("Threshold down should be greater than zero");
            
            RuleFor(o => o.ThresholdUp)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Threshold up should be greater than or equal to zero")
                .Must((model, value) => model.ThresholdDown <= value)
                .WithMessage("Threshold up should be greater than or equal to threshold down");

            RuleFor(o => o.MarketOrderMarkup)
                .InclusiveBetween(0, 1)
                .WithMessage("The market order markup should be between 0 and 100%");
        }
    }
}
