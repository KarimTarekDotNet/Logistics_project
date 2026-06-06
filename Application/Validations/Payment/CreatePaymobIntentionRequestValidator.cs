using Application.DTOs.Payment;
using FluentValidation;

namespace Application.Validations.Payment
{
    public class CreatePaymobIntentionRequestValidator
        : AbstractValidator<CreatePaymobIntentionRequest>
    {
        public CreatePaymobIntentionRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.Currency)
                .NotEmpty()
                .MaximumLength(10);

            RuleFor(x => x.PaymentMethods)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.Items)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.BillingData)
                .NotNull();

            RuleFor(x => x.SpecialReference)
                .NotEmpty()
                .MaximumLength(100);

            RuleForEach(x => x.Items)
                .SetValidator(new PaymobItemRequestValidator());

            RuleFor(x => x.BillingData)
                .SetValidator(new PaymobBillingDataRequestValidator());
        }
    }
}
