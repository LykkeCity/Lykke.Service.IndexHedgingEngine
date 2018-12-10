using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.HedgeLimitOrders;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class HedgeLimitOrderCancelModelValidator : AbstractValidator<HedgeLimitOrderCancelModel>
    {
        public HedgeLimitOrderCancelModelValidator()
        {
            RuleFor(o => o.AssetId)
                .NotEmpty()
                .WithMessage("Asset id required");

            RuleFor(o => o.Exchange)
                .NotEmpty()
                .WithMessage("Exchange required");
        }
    }
}
