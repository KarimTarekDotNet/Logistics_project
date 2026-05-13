using Application.DTOs.Shipments.Core;
using FluentValidation;

namespace Application.Validations.Shipments.Core
{
    public class UpdateShipmentTrackingValidator : AbstractValidator<UpdateShipmentTrackingRequest>
    {
        public UpdateShipmentTrackingValidator()
        {
            RuleFor(x => x.BookingNumber)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.BookingNumber))
                .WithMessage("BookingNumber must not exceed 100 characters.");

            RuleFor(x => x.VesselName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.VesselName))
                .WithMessage("VesselName must not exceed 200 characters.");

            RuleFor(x => x.VoyageNumber)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.VoyageNumber))
                .WithMessage("VoyageNumber must not exceed 100 characters.");

            RuleFor(x => x.CurrentCheckpoint)
                .MaximumLength(250)
                .When(x => !string.IsNullOrWhiteSpace(x.CurrentCheckpoint))
                .WithMessage("CurrentCheckpoint must not exceed 250 characters.");

            RuleFor(x => x.EstimatedArrival)
                .GreaterThan(x => x.EstimatedDeparture!.Value)
                .When(x => x.EstimatedDeparture.HasValue && x.EstimatedArrival.HasValue)
                .WithMessage("EstimatedArrival must be after EstimatedDeparture.");

            RuleFor(x => x.ActualArrival)
                .GreaterThan(x => x.ActualDeparture!.Value)
                .When(x => x.ActualDeparture.HasValue && x.ActualArrival.HasValue)
                .WithMessage("ActualArrival must be after ActualDeparture.");

            RuleFor(x => x.ActualDeparture)
                .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddDays(1))
                .When(x => x.ActualDeparture.HasValue)
                .WithMessage("ActualDeparture cannot be in the far future.");

            RuleFor(x => x.ActualArrival)
                .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddDays(1))
                .When(x => x.ActualArrival.HasValue)
                .WithMessage("ActualArrival cannot be in the far future.");
        }
    }
}