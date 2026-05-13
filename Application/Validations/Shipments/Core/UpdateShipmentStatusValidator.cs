using Application.DTOs.Shipments.Core;
using FluentValidation;

namespace Application.Validations.Shipments.Core
{
    public class UpdateShipmentStatusValidator : AbstractValidator<UpdateShipmentStatusHistoryRequest>
    {
        public UpdateShipmentStatusValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty()
                .When(x => x.ShipmentId.HasValue)
                .WithMessage("ShipmentId is required.");

            RuleFor(x => x.FromStatus)
                .NotEmpty()
                .When(x => x.FromStatus is not null)
                .WithMessage("FromStatus is required.")
                .MaximumLength(50)
                .When(x => x.FromStatus is not null)
                .WithMessage("FromStatus must not exceed 50 characters.");

            RuleFor(x => x.ToStatus)
                .NotEmpty()
                .When(x => x.ToStatus is not null)
                .WithMessage("ToStatus is required.")
                .MaximumLength(50)
                .When(x => x.ToStatus is not null)
                .WithMessage("ToStatus must not exceed 50 characters.");

            RuleFor(x => x.ChangedBy)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.ChangedBy))
                .WithMessage("ChangedBy must not exceed 100 characters.");

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Reason))
                .WithMessage("Reason must not exceed 500 characters.");
        }
    }
}