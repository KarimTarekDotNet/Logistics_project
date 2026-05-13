using Application.DTOs.Shipments.Core;
using FluentValidation;

namespace Application.Validations.Shipments.Core
{
    public class CreateShipmentChargeValidator : AbstractValidator<CreateShipmentChargeRequest>
    {
        public CreateShipmentChargeValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty().WithMessage("ShipmentId is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(250).WithMessage("Description must not exceed 250 characters.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.TaxAmount)
                .GreaterThan(0).WithMessage("Tax amount must be greater than 0.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .MaximumLength(5).WithMessage("Currency must not exceed 5 characters.");
        }
    }
}