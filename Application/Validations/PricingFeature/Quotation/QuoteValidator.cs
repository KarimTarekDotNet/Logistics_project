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
                .NotEmpty().WithMessage("Container type is required.");

            RuleFor(q => q.Items)
                .NotEmpty().WithMessage("Quote must have at least one item.");

            RuleForEach(q => q.Items)
                .SetValidator(new CreateQuoteItemRequestValidator());
        }
    }

    public class UpdateQuoteRequestValidator : AbstractValidator<UpdateQuoteRequest>
    {
        public UpdateQuoteRequestValidator()
        {
            RuleFor(q => q.CustomerName)
                .NotEmpty().WithMessage("Customer name is required.")
                .MaximumLength(100).WithMessage("Customer name must not exceed 100 characters.");

            RuleFor(q => q.FinalPrice)
                .GreaterThan(0).WithMessage("Final price must be greater than zero.");

            RuleFor(q => q.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .MaximumLength(4).WithMessage("Currency must not exceed 4 characters.")
                .Must(RateRules.IsValidCurrency)
                .WithMessage($"Currency must be one of: {string.Join(", ", RateRules.AllowedCurrencies)}.");
        }
    }
}
