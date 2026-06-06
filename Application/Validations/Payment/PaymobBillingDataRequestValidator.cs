using Application.DTOs.Payment;
using FluentValidation;

namespace Application.Validations.Payment
{
    public class PaymobBillingDataRequestValidator
    : AbstractValidator<PaymobBillingDataRequest>
    {
        public PaymobBillingDataRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Country)
                .NotEmpty()
                .Length(2);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}
