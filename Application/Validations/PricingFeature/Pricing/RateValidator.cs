using Application.ApplicationRules;
using Application.DTOs.Pricing.PricingEngine;
using FluentValidation;

namespace Application.Validations.PricingFeature.Pricing
{
    public class CreateRateRequestValidator : AbstractValidator<CreateRateRequest>
    {
        public CreateRateRequestValidator()
        {
            RuleFor(r => r.CarrierId)
                .NotEmpty().WithMessage("Carrier is required.");

            RuleFor(r => r.RouteId)
                .NotEmpty().WithMessage("Route is required.");

            RuleFor(r => r.ContainerTypeId)
                .NotEmpty().WithMessage("Container type is required.");

            RuleFor(r => r.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(r => r.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .MaximumLength(4).WithMessage("Currency must not exceed 4 characters.")
                .Must(RateRules.IsValidCurrency)
                .WithMessage($"Currency must be one of: {string.Join(", ", RateRules.AllowedCurrencies)}.");

            RuleFor(r => r.ValidFrom)
                .NotEmpty().WithMessage("Valid from date is required.");

            RuleFor(r => r.ValidTo)
                .NotEmpty().WithMessage("Valid to date is required.")
                .Must((r, validTo) => RateRules.IsValidDateRange(r.ValidFrom, validTo))
                .WithMessage("Valid to date must be after valid from date.");
        }
    }

    public class UpdateRateRequestValidator : AbstractValidator<UpdateRateRequest>
    {
        public UpdateRateRequestValidator()
        {
            RuleFor(r => r.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(r => r.Currency)
                .MaximumLength(4).WithMessage("Currency must not exceed 4 characters.");

            RuleFor(r => r.ValidFrom);

            RuleFor(r => r.ValidTo)
                .Must((r, validTo) => RateRules.IsValidDateRange(r.ValidFrom, validTo))
                .WithMessage("Valid to date must be after valid from date.");
        }
    }
}
