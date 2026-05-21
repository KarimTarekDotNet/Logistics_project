using Application.ApplicationRules;
using Application.DTOs.Pricing.Quotation;
using FluentValidation;

namespace Application.Validations.PricingFeature.Quotation
{
    public class CreateQuoteRequestValidator : AbstractValidator<CreateQuoteRequest>
    {
        public CreateQuoteRequestValidator()
        {
            RuleFor(q => q.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required.");

            RuleFor(q => q.RateId)
                .NotEmpty().WithMessage("Rate ID is required.");

            RuleFor(q => q.RequestedGrossWeightKg)
                .GreaterThan(0).WithMessage("Gross weight must be greater than zero.");

            RuleFor(q => q.RequestedNetWeightKg)
                .GreaterThan(0).WithMessage("Net weight must be greater than zero.");

            RuleFor(q => q.RequestedVolumeCbm)
                .GreaterThan(0).WithMessage("Volume CBM must be greater than zero.");

            RuleFor(q => q)
                .Must(q => q.RequestedNetWeightKg <= q.RequestedGrossWeightKg)
                .WithMessage("Net weight cannot be greater than gross weight.");

            RuleFor(q => q.RequiredTemperatureCelsius)
                .InclusiveBetween(-60m, 60m)
                .When(q => q.RequiredTemperatureCelsius.HasValue)
                .WithMessage("Required temperature must be between -60 and 60 Celsius.");
        }
    }
}
