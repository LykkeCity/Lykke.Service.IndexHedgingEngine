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
            RuleFor(o => o.ThresholdDownBuy)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Threshold down buy should be greater than or equal to zero");

            RuleFor(o => o.ThresholdDownSell)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Threshold down sell should be greater than or equal to zero");

            RuleFor(o => o.ThresholdUpBuy)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Threshold up buy should be greater than or equal to zero")
                .Must((model, value) => model.ThresholdDownBuy <= value)
                .WithMessage("Threshold up buy should be greater than or equal to threshold down buy");

            RuleFor(o => o.ThresholdUpSell)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Threshold up sell should be greater than or equal to zero")
                .Must((model, value) => model.ThresholdDownSell <= value)
                .WithMessage("Threshold up sell should be greater than or equal to threshold down sell");

            RuleFor(o => o.ThresholdCritical)
                .GreaterThan(0)
                .WithMessage("Threshold critical should be greater than zero");
            
            RuleFor(o => o.MarketOrderMarkup)
                .InclusiveBetween(0, 1)
                .WithMessage("The market order markup should be between 0 and 100%");
        }
    }
}
