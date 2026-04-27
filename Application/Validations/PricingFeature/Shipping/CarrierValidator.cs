using Application.DTOs.ShippingCore;
using FluentValidation;

namespace Application.Validations.PricingFeature.Shipping
{
    public class CreateCarrierRequestValidator : AbstractValidator<CreateCarrierRequest>
    {
        public CreateCarrierRequestValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Carrier name is required.")
                .MaximumLength(100).WithMessage("Carrier name must not exceed 100 characters.");

            RuleFor(c => c.Code)
                .NotEmpty().WithMessage("Carrier code is required.")
                .MaximumLength(10).WithMessage("Carrier code must not exceed 10 characters.")
                .Matches("^[A-Z]{4}$").WithMessage("Carrier code must be exactly 4 uppercase letters (SCAC format).");
        }
    }

    public class UpdateCarrierRequestValidator : AbstractValidator<UpdateCarrierRequest>
    {
        public UpdateCarrierRequestValidator()
        {
            RuleFor(c => c.Name)
                .MaximumLength(100).WithMessage("Carrier name must not exceed 100 characters.");

            RuleFor(c => c.Code)
                .MaximumLength(10).WithMessage("Carrier code must not exceed 10 characters.");
        }
    }
}
