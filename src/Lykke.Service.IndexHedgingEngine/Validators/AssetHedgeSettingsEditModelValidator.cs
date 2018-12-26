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
        }
    }
}
