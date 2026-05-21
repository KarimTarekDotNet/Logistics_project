using Application.ApplicationRules;
using Application.DTOs.Pricing.PricingEngine.Rates;
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

            RuleFor(r => r.MaxGrossWeightKg)
                .GreaterThan(0).WithMessage("Max gross weight must be greater than zero.")
                .When(r => r.MaxGrossWeightKg.HasValue);

            RuleFor(r => r.MaxNetWeightKg)
                .GreaterThan(0).WithMessage("Max net weight must be greater than zero.")
                .When(r => r.MaxNetWeightKg.HasValue);

            RuleFor(r => r.MaxVolumeCbm)
                .GreaterThan(0).WithMessage("Max volume CBM must be greater than zero.")
                .When(r => r.MaxVolumeCbm.HasValue);

            RuleFor(r => r)
                .Must(r => !r.MaxNetWeightKg.HasValue ||
                           !r.MaxGrossWeightKg.HasValue ||
                           r.MaxNetWeightKg <= r.MaxGrossWeightKg)
                .WithMessage("Max net weight cannot be greater than max gross weight.");

            RuleFor(r => r)
                .Must(r => !r.MinTemperatureCelsius.HasValue ||
                           !r.MaxTemperatureCelsius.HasValue ||
                           r.MinTemperatureCelsius <= r.MaxTemperatureCelsius)
                .When(r => r.AllowsHazardous == true)
                .WithMessage("Minimum temperature cannot be greater than maximum temperature.");
        }
    }

    public class UpdateRateRequestValidator : AbstractValidator<UpdateRateRequest>
    {
        public UpdateRateRequestValidator()
        {
            RuleFor(r => r.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(r => r.Currency)
                .MaximumLength(4).WithMessage("Currency must not exceed 4 characters.")
                .Must(currency => string.IsNullOrWhiteSpace(currency) || RateRules.IsValidCurrency(currency))
                .WithMessage($"Currency must be one of: {string.Join(", ", RateRules.AllowedCurrencies)}.");

            RuleFor(r => r.ValidTo)
                .Must((r, validTo) => RateRules.IsValidDateRange(r.ValidFrom, validTo))
                .WithMessage("Valid to date must be after valid from date.");

            RuleFor(r => r.MaxGrossWeightKg)
                .GreaterThan(0).WithMessage("Max gross weight must be greater than zero.")
                .When(r => r.MaxGrossWeightKg.HasValue);

            RuleFor(r => r.MaxNetWeightKg)
                .GreaterThan(0).WithMessage("Max net weight must be greater than zero.")
                .When(r => r.MaxNetWeightKg.HasValue);

            RuleFor(r => r.MaxVolumeCbm)
                .GreaterThan(0).WithMessage("Max volume CBM must be greater than zero.")
                .When(r => r.MaxVolumeCbm.HasValue);

            RuleFor(r => r)
                .Must(r => !r.MaxNetWeightKg.HasValue ||
                           !r.MaxGrossWeightKg.HasValue ||
                           r.MaxNetWeightKg <= r.MaxGrossWeightKg)
                .WithMessage("Max net weight cannot be greater than max gross weight.");

            RuleFor(r => r)
                .Must(r => !r.MinTemperatureCelsius.HasValue ||
                           !r.MaxTemperatureCelsius.HasValue ||
                           r.MinTemperatureCelsius <= r.MaxTemperatureCelsius)
                .When(r => r.AllowsHazardous == true)
                .WithMessage("Minimum temperature cannot be greater than maximum temperature.");
        }
    }

    public class QueryMarketRequestValidator : AbstractValidator<QueryMarketRequest>
    {
        public QueryMarketRequestValidator()
        {
            RuleFor(r => r.RouteId)
                .NotEmpty().WithMessage("Route is required.");

            RuleFor(r => r.ContainerId)
                .NotEmpty().WithMessage("Container type is required.");

            RuleFor(r => r.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .MaximumLength(4).WithMessage("Currency must not exceed 4 characters.")
                .Must(RateRules.IsValidCurrency)
                .WithMessage($"Currency must be one of: {string.Join(", ", RateRules.AllowedCurrencies)}.");
        }
    }
}