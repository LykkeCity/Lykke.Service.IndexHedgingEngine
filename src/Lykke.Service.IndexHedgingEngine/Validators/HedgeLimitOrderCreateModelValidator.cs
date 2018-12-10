using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models;
using Lykke.Service.IndexHedgingEngine.Client.Models.HedgeLimitOrders;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class HedgeLimitOrderCreateModelValidator : AbstractValidator<HedgeLimitOrderCreateModel>
    {
        public HedgeLimitOrderCreateModelValidator()
        {
            RuleFor(o => o.AssetId)
                .NotEmpty()
                .WithMessage("Asset id required");

            RuleFor(o => o.Exchange)
                .NotEmpty()
                .WithMessage("Exchange required");

            RuleFor(o => o.Type)
                .NotEqual(LimitOrderType.None)
                .WithMessage("Limit order type should be specified");

            RuleFor(o => o.Price)
                .GreaterThan(decimal.Zero)
                .WithMessage("Price should be greater than zero");

            RuleFor(o => o.Volume)
                .GreaterThan(decimal.Zero)
                .WithMessage("Volume should be greater than zero");
        }
    }
}
