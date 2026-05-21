using Application.DTOs.Shipments.Core;
using FluentValidation;

namespace Application.Validations.Shipments.Core
{
    public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceRequest>
    {
        public CreateInvoiceValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty()
                .WithMessage("ShipmentId is required.");

            RuleFor(x => x.ShipmentChargeIds)
                .NotNull()
                .WithMessage("ShipmentChargeIds cannot be null.");

            RuleFor(x => x.ShipmentChargeIds)
                .Must(x => x.Distinct().Count() == x.Count)
                .WithMessage("Duplicate shipment charge ids are not allowed.");

            RuleForEach(x => x.ShipmentChargeIds)
                .NotEmpty()
                .WithMessage("Shipment charge id cannot be empty.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .MaximumLength(10)
                .WithMessage("Currency length must not exceed 10 characters.");

            RuleFor(x => x.PayerType)
                .IsInEnum()
                .WithMessage("Invalid payer type.");

            RuleFor(x => x.DueDate)
                .GreaterThan(DateTimeOffset.UtcNow)
                .WithMessage("Due date must be in the future.");
        }
    }
    public class PriceRequestValidator : AbstractValidator<PriceRequest>
    {
        public PriceRequestValidator()
        {
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");
        }
    }
}