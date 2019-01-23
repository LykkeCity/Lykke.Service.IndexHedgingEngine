using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.AssetHedgeSettings;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class AssetHedgeSettingsEditModelValidator : AbstractValidator<AssetHedgeSettingsModel>
    {
        public AssetHedgeSettingsEditModelValidator()
        {
            RuleFor(o => o.AssetId)
                .NotEmpty()
                .WithMessage("Asset id required");

            RuleFor(o => o.Exchange)
                .NotEmpty()
                .WithMessage("Exchange required");

            RuleFor(o => o.AssetPairId)
                .NotEmpty()
                .WithMessage("Asset pair id required");

            RuleFor(o => o.Mode)
                .NotEqual(AssetHedgeMode.None)
                .WithMessage("Hedging mode should be specified");

            RuleFor(o => o.ReferenceDelta)
                .Must((model, value) => value == null || 0 <= value && value <= 1)
                .WithMessage("The reference delta should be between 0 and 1");

            RuleFor(o => o.ThresholdUp)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Threshold up should be greater than or equal to zero");

            RuleFor(o => o.ThresholdDown)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Threshold down should be greater than or equal to zero");

            RuleFor(o => o.ThresholdCritical)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Threshold critical should be greater than or equal to zero");
        }
    }
}
