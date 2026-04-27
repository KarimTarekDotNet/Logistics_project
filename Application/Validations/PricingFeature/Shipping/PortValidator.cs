using Application.DTOs.ShippingCore;
using FluentValidation;

namespace Application.Validations.PricingFeature.Shipping
{
    public class CreatePortRequestValidator : AbstractValidator<CreatePortRequest>
    {
        public CreatePortRequestValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Port name is required.")
                .MaximumLength(100).WithMessage("Port name must not exceed 100 characters.");

            RuleFor(p => p.Country)
                .NotEmpty().WithMessage("Country is required.")
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

            RuleFor(p => p.Code)
                .NotEmpty().WithMessage("Port code is required.")
                .MaximumLength(10).WithMessage("Port code must not exceed 10 characters.")
                .Matches("^[A-Z]{2}\\s?[A-Z0-9]{3}$").WithMessage("Port code must follow UN/LOCODE format (e.g. CNSHA).");
        }
    }

    public class UpdatePortRequestValidator : AbstractValidator<UpdatePortRequest>
    {
        public UpdatePortRequestValidator()
        {
            RuleFor(p => p.Name)
                .MaximumLength(100).WithMessage("Port name must not exceed 100 characters.");

            RuleFor(p => p.Country)
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

            RuleFor(p => p.Code)
                .MaximumLength(10).WithMessage("Port code must not exceed 10 characters.")
                .Matches("^[A-Z]{2}\\s?[A-Z0-9]{3}$").WithMessage("Port code must follow UN/LOCODE format (e.g. CNSHA).");
        }
    }
}
