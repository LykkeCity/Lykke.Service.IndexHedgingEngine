using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.IndexSettings;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class IndexSettingsModelValidator : AbstractValidator<IndexSettingsModel>
    {
        public IndexSettingsModelValidator()
        {
            RuleFor(o => o.Name)
                .NotEmpty()
                .WithMessage("Name required");

            RuleFor(o => o.AssetId)
                .NotEmpty()
                .WithMessage("Asset id required");

            RuleFor(o => o.AssetPairId)
                .NotEmpty()
                .WithMessage("Asset pair id required");

            RuleFor(o => o.Alpha)
                .InclusiveBetween(0, 1)
                .WithMessage("Alpha should be between 0 and 100%");

            RuleFor(o => o.TrackingFee)
                .InclusiveBetween(0, 1)
                .WithMessage("Tracking fee should be between 0 and 100%");

            RuleFor(o => o.PerformanceFee)
                .InclusiveBetween(0, 1)
                .WithMessage("Performance fee should be between 0 and 100%");

            RuleFor(o => o.SellMarkup)
                .GreaterThan(0)
                .WithMessage("Sell markup should greater than 0");

            RuleFor(o => o.SellVolume)
                .GreaterThan(0)
                .WithMessage("Sell volume should greater than 0");

            RuleFor(o => o.BuyVolume)
                .GreaterThan(0)
                .WithMessage("Buy volume should greater than 0");

            RuleFor(o => o.SellLimitOrdersCount)
                .GreaterThan(0)
                .WithMessage("Sell limit orders count should greater than 0");

            RuleFor(o => o.BuyLimitOrdersCount)
                .GreaterThan(0)
                .WithMessage("Buy limit orders count should greater than 0");
        }
    }
}
