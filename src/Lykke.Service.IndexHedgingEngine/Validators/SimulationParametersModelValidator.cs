using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.IndexHedgingEngine.Client.Models.Simulation;

namespace Lykke.Service.IndexHedgingEngine.Validators
{
    [UsedImplicitly]
    public class SimulationParametersModelValidator : AbstractValidator<SimulationParametersModel>
    {
        public SimulationParametersModelValidator()
        {
            RuleFor(o => o.IndexName)
                .NotEmpty()
                .WithMessage("Index name required");

            RuleFor(o => o.OpenTokens)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Open tokens should be greater than zero");

            RuleFor(o => o.Investments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Investments should be greater than zero");
        }
    }
}
