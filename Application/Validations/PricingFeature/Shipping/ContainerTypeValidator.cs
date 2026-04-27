using Application.DTOs.ShippingCore;
using FluentValidation;

namespace Application.Validations.PricingFeature.Shipping
{
    public class CreateContainerTypeRequestValidator : AbstractValidator<CreateContainerTypeRequest>
    {
        public CreateContainerTypeRequestValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Container type name is required.")
                .MaximumLength(50).WithMessage("Container type name must not exceed 50 characters.");
        }
    }

    public class UpdateContainerTypeRequestValidator : AbstractValidator<UpdateContainerTypeRequest>
    {
        public UpdateContainerTypeRequestValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Container type name is required.")
                .MaximumLength(50).WithMessage("Container type name must not exceed 50 characters.");
        }
    }
}
