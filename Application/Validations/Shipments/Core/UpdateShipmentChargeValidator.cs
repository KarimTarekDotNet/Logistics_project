using Application.DTOs.Shipments.Core;
using FluentValidation;

namespace Application.Validations.Shipments.Core
{
    public class UpdateShipmentChargeValidator : AbstractValidator<UpdateShipmentChargeRequest>
    {
        public UpdateShipmentChargeValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description is required.")
                .MaximumLength(250)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description must not exceed 250 characters.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .When(x => x.Amount.HasValue)
                .WithMessage("Amount must be greater than 0.");
        }
    }
}