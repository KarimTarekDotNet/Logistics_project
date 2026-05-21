using Application.DTOs.Shipments.Core;
using FluentValidation;

namespace Application.Validations.Shipments.Core
{
    public class CreateShipmentItemValidator : AbstractValidator<CreateShipmentItemRequest>
    {
        public CreateShipmentItemValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty()
                .WithMessage("ShipmentId is required.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MaximumLength(250)
                .WithMessage("Description must not exceed 250 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.GrossWeight)
                .GreaterThan(0)
                .WithMessage("GrossWeight must be greater than 0.");

            RuleFor(x => x.NetWeight)
                .GreaterThan(0)
                .WithMessage("NetWeight must be greater than 0.");

            RuleFor(x => x.VolumeCbm)
                .GreaterThanOrEqualTo(0)
                .WithMessage("VolumeCbm cannot be negative.");

            RuleFor(x => x.RequiredTemperatureCelsius)
                .InclusiveBetween(-50, 50)
                .When(x => x.RequiredTemperatureCelsius.HasValue)
                .WithMessage("RequiredTemperatureCelsius must be between -50 and 50.");

            RuleFor(x => x.MarksAndNumbers)
                .MaximumLength(200)
                .WithMessage("MarksAndNumbers must not exceed 200 characters.");

            RuleFor(x => x)
                .Must(x => x.GrossWeight >= x.NetWeight)
                .WithMessage("GrossWeight must be greater than or equal to NetWeight.");
        }
    }
}