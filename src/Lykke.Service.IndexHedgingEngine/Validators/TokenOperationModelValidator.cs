using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models;
using Lykke.Service.IndexHedgingEngine.Client.Models.Tokens;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class TokenOperationModelValidator : AbstractValidator<TokenOperationModel>
    {
        public TokenOperationModelValidator()
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

            RuleFor(o => o.UserId)
                .NotEmpty()
                .WithMessage("User id required");
        }
    }
}
