using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Settlements;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class SettlementRequestModelValidator : AbstractValidator<SettlementRequestModel>
    {
        public SettlementRequestModelValidator()
        {
            RuleFor(o => o.IndexName)
                .NotEmpty()
                .WithMessage("Index name required");
            
            RuleFor(o => o.Amount)
                .GreaterThan(0)
                .WithMessage("Amount should be greater than zero");
            
            RuleFor(o => o.ClientId)
                .NotEmpty()
                .WithMessage("Client id required");
        }
    }
}
