using Application.DTOs.Pricing.Imports;
using FluentValidation;

namespace Application.Validations.PricingFeature.Imports
{
    public class ImportRatesRequestValidator : AbstractValidator<ImportRatesRequest>
    {
        public ImportRatesRequestValidator()
        {
            RuleFor(x => x.Source)
                .NotEmpty().IsInEnum().WithMessage("Source is required.");

            RuleFor(x => x.Rates)
                .NotNull().WithMessage("Rates list cannot be null.")
                .NotEmpty().WithMessage("At least one rate is required.");

            RuleForEach(x => x.Rates)
                .SetValidator(new ImportRateItemRequestValidator());
        }
    }

    public class ImportRateItemRequestValidator : AbstractValidator<ImportRateItemRequest>
    {
        public ImportRateItemRequestValidator()
        {
            RuleFor(x => x.ExternalMessageId)
                .NotEmpty().WithMessage("ExternalMessageId is required.")
                .MaximumLength(100);

            RuleFor(x => x.CarrierName)
                .NotEmpty().WithMessage("CarrierName is required.")
                .MaximumLength(100);

            RuleFor(x => x.FromPortCode)
                .NotEmpty().WithMessage("FromPortCode is required.")
                .MaximumLength(10);

            RuleFor(x => x.ToPortCode)
                .NotEmpty().WithMessage("ToPortCode is required.")
                .MaximumLength(10)
                .NotEqual(x => x.FromPortCode)
                .WithMessage("FromPortCode and ToPortCode cannot be the same.");

            RuleFor(x => x.ContainerTypeName)
                .NotEmpty().WithMessage("ContainerTypeName is required.")
                .MaximumLength(50);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be 3 characters (ISO code).");

            RuleFor(x => x.ValidFrom)
                .LessThan(x => x.ValidTo)
                .WithMessage("ValidFrom must be earlier than ValidTo.");

            RuleFor(x => x.ValidTo)
                .GreaterThan(DateTimeOffset.UtcNow.AddYears(-1))
                .WithMessage("ValidTo seems too old.");

            RuleFor(x => x.RawSubject)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.RawSubject));
        }
    }
}
