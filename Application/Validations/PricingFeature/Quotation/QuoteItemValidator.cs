using Application.DTOs.Pricing.Quotation;
using FluentValidation;

namespace Application.Validations.PricingFeature.Quotation
{
    public class CreateQuoteItemRequestValidator : AbstractValidator<CreateQuoteItemRequest>
    {
        public CreateQuoteItemRequestValidator()
        {
            RuleFor(i => i.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");

            RuleFor(i => i.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        }
    }

    public class UpdateQuoteItemRequestValidator : AbstractValidator<UpdateQuoteItemRequest>
    {
        public UpdateQuoteItemRequestValidator()
        {
            RuleFor(i => i.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");

            RuleFor(i => i.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        }
    }
}
