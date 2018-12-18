using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Assets;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class AssetSettingsModelValidator : AbstractValidator<AssetSettingsModel>
    {
        public AssetSettingsModelValidator()
        {
            RuleFor(o => o.Asset)
                .NotEmpty()
                .WithMessage("Asset required");

            RuleFor(o => o.Exchange)
                .NotEmpty()
                .WithMessage("Exchange required");

            RuleFor(o => o.AssetId)
                .NotEmpty()
                .WithMessage("Asset id required");

            RuleFor(o => o.Accuracy)
                .InclusiveBetween(1, 8)
                .WithMessage("Accuracy should be between 1 and 8");
        }
    }
}
