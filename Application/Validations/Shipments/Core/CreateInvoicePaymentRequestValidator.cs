using Application.DTOs.Shipments.Core;
using FluentValidation;

namespace Application.Validations.Shipments.Core
{
    public class CreateInvoicePaymentRequestValidator : AbstractValidator<CreateInvoicePaymentRequest>
    {
        public CreateInvoicePaymentRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Payment amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3)
                .WithMessage("Currency must be a valid 3-letter ISO code.")
                .Matches("^[A-Z]{3}$")
                .WithMessage("Currency must be uppercase, e.g. EGP.");

            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.ReferenceNumber));
        }
    }
}
