using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models;
using Lykke.Service.IndexHedgingEngine.Client.Models.Balances;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class AssetBalanceOperationModelValidator : AbstractValidator<AssetBalanceOperationModel>
    {
        public AssetBalanceOperationModelValidator()
        {
            RuleFor(o => o.AssetId)
                .NotEmpty()
                .WithMessage("Asset id required");

            RuleFor(o => o.Amount)
                .GreaterThan(0)
                .WithMessage("Amount should be greater than zero");

            RuleFor(o => o.Type)
                .NotEqual(BalanceOperationType.None)
                .WithMessage("Type should be specified");

            RuleFor(o => o.Comment)
                .NotEmpty()
                .WithMessage("Comment required");
        }
    }
}
