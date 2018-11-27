using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.AssetLinks;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class AssetLinkModelValidator : AbstractValidator<AssetLinkModel>
    {
        public AssetLinkModelValidator()
        {
            RuleFor(o => o.AssetId)
                .NotEmpty()
                .WithMessage("Asset id required");

            RuleFor(o => o.LykkeAssetId)
                .NotEmpty()
                .WithMessage("Lykke asset id required");
        }
    }
}
