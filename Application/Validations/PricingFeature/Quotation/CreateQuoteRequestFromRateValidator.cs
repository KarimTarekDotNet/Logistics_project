using Application.DTOs.Pricing.Quotation;
using FluentValidation;

namespace Application.Validations.PricingFeature.Quotation
{
    public class CreateQuoteRequestFromRateValidator : AbstractValidator<CreateQuoteRequestFromRate>
    {
        public CreateQuoteRequestFromRateValidator()
        {
            RuleFor(x => x.RateId)
                .NotEmpty()
                .WithMessage("Rate ID is required.");

            RuleFor(x => x.RequestedGrossWeightKg)
                .GreaterThan(0)
                .WithMessage("Gross weight must be greater than zero.");

            RuleFor(x => x.RequestedNetWeightKg)
                .GreaterThan(0)
                .WithMessage("Net weight must be greater than zero.");

            RuleFor(x => x.RequestedVolumeCbm)
                .GreaterThan(0)
                .WithMessage("Volume must be greater than zero.");

            RuleFor(x => x.RequestedNetWeightKg)
                .LessThanOrEqualTo(x => x.RequestedGrossWeightKg)
                .WithMessage("Net weight cannot exceed gross weight.");

            RuleFor(x => x.RequiredTemperatureCelsius)
                .InclusiveBetween(-50, 50)
                .When(x => x.RequiredTemperatureCelsius.HasValue)
                .WithMessage("Temperature must be between -50 and 50 Celsius.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters.");
        }
    }
    public class RejectQuoteRequestValidator : AbstractValidator<RejectQuoteRequest>
    {
        public RejectQuoteRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Rejection reason is required.")

                .MinimumLength(5)
                .WithMessage("Rejection reason is too short.")

                .MaximumLength(500)
                .WithMessage("Rejection reason cannot exceed 500 characters.")

                .Matches(@"^(?!\s*$)[a-zA-Z0-9\u0600-\u06FF\s\.,\-_()]+$")
                .WithMessage("Rejection reason contains invalid characters.");
        }
    }
}
