using Application.DTOs.Shipments.Core;
using FluentValidation;

namespace Application.Validations.Shipments.Core
{
    public class CreateShipmentStatusValidator : AbstractValidator<CreateShipmentStatusHistoryRequest>
    {
        public CreateShipmentStatusValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty().WithMessage("ShipmentId is required.");

            RuleFor(x => x.FromStatus)
                .NotEmpty().WithMessage("FromStatus is required.")
                .MaximumLength(50).WithMessage("FromStatus must not exceed 50 characters.");

            RuleFor(x => x.ToStatus)
                .NotEmpty().WithMessage("ToStatus is required.")
                .MaximumLength(50).WithMessage("ToStatus must not exceed 50 characters.");

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