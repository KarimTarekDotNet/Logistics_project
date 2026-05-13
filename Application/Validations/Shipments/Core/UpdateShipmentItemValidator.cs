using Application.DTOs.Shipments.Core;
using FluentValidation;

namespace Application.Validations.Shipments.Core
{
    public class UpdateShipmentItemValidator : AbstractValidator<UpdateShipmentItemRequest>
    {
        public UpdateShipmentItemValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty()
                .When(x => x.ShipmentId.HasValue)
                .WithMessage("ShipmentId is invalid.");

            RuleFor(x => x.Description)
                .MaximumLength(250)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description must not exceed 250 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .When(x => x.Quantity.HasValue)
                .WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.ChargeableWeight)
                .GreaterThan(0)
                .When(x => x.ChargeableWeight.HasValue)
                .WithMessage("ChargeableWeight must be greater than 0.");

            RuleFor(x => x.GrossWeight)
                .GreaterThan(0)
                .When(x => x.GrossWeight.HasValue)
                .WithMessage("GrossWeight must be greater than 0.");

            RuleFor(x => x.NetWeight)
                .GreaterThan(0)
                .When(x => x.NetWeight.HasValue)
                .WithMessage("NetWeight must be greater than 0.");

            RuleFor(x => x.VolumeCbm)
                .GreaterThanOrEqualTo(0)
                .When(x => x.VolumeCbm.HasValue)
                .WithMessage("VolumeCbm cannot be negative.");

            RuleFor(x => x.RequiredTemperatureCelsius)
                .InclusiveBetween(-50, 50)
                .When(x => x.RequiredTemperatureCelsius.HasValue)
                .WithMessage("RequiredTemperatureCelsius must be between -50 and 50.");

            RuleFor(x => x.MarksAndNumbers)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.MarksAndNumbers))
                .WithMessage("MarksAndNumbers must not exceed 200 characters.");

            RuleFor(x => x)
                .Must(x =>
                {
                    if (x.GrossWeight.HasValue && x.NetWeight.HasValue)
                        return x.GrossWeight >= x.NetWeight;

                    return true;
                })
                .WithMessage("GrossWeight must be greater than or equal to NetWeight.");
        }
    }
}