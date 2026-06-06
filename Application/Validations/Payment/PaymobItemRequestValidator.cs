using Application.DTOs.Payment;
using FluentValidation;

namespace Application.Validations.Payment
{
    public class PaymobItemRequestValidator
    : AbstractValidator<PaymobItemRequest>
    {
        public PaymobItemRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
