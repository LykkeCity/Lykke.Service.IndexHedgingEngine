using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models;
using Lykke.Service.IndexHedgingEngine.Client.Models.Funding;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class FundingOperationModelValidator : AbstractValidator<FundingOperationModel>
    {
        public FundingOperationModelValidator()
        {
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
