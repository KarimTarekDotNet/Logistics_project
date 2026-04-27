using Application.DTOs.ShippingCore;
using FluentValidation;

namespace Application.Validations.PricingFeature.Shipping
{
    public class CreateRouteRequestValidator : AbstractValidator<CreateRouteRequest>
    {
        public CreateRouteRequestValidator()
        {
            RuleFor(r => r.FromPortId)
                .NotEmpty().WithMessage("Origin port is required.");

            RuleFor(r => r.ToPortId)
                .NotEmpty().WithMessage("Destination port is required.")
                .NotEqual(r => r.FromPortId).WithMessage("Destination port must differ from origin port.");
        }
    }

    public class UpdateRouteRequestValidator : AbstractValidator<UpdateRouteRequest>
    {
        public UpdateRouteRequestValidator()
        {
            RuleFor(r => r.FromPortId)
                .NotEmpty().WithMessage("Origin port is required.");

            RuleFor(r => r.ToPortId)
                .NotEmpty().WithMessage("Destination port is required.")
                .NotEqual(r => r.FromPortId).WithMessage("Destination port must differ from origin port.");
        }
    }
}
