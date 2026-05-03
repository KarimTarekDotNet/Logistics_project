using Application.DTOs.Shipments.Core;
using FluentValidation;

namespace Application.Validations.Shipments.Core
{
    public class CreateShipmentValidator : AbstractValidator<CreateShipmentRequest>
    {
        public CreateShipmentValidator()
        {
            RuleFor(x => x.QuoteId)
                .NotEmpty().WithMessage("Quote Id is required.");

            RuleFor(x => x.CarrierId)
                .NotEmpty().WithMessage("Carrier Id is required.");
        }
    }

    public class UpdateShipmentValidator : AbstractValidator<UpdateShipmentRequest>
    {
        public UpdateShipmentValidator()
        {
            RuleFor(x => x.QuoteId)
            .Must(id => id != Guid.Empty)
            .When(x => x.QuoteId.HasValue)
            .WithMessage("QuoteId must be a valid GUID.");

            RuleFor(x => x.CarrierId)
            .Must(id => id != Guid.Empty)
            .When(x => x.CarrierId.HasValue)
            .WithMessage("CarrierId must be a valid GUID.");
        }
    }

    public class CreateShipmentItemValidator : AbstractValidator<CreateShipmentItemRequest>
    {
        public CreateShipmentItemValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty().WithMessage("ShipmentId is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(250).WithMessage("Description must not exceed 250 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Weight must be greater than 0.");
        }
    }

    public class UpdateShipmentItemValidator : AbstractValidator<UpdateShipmentItemRequest>
    {
        public UpdateShipmentItemValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty()
                .When(x => x.ShipmentId.HasValue)
                .WithMessage("ShipmentId is required.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description is required.")
                .MaximumLength(250)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description must not exceed 250 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .When(x => x.Quantity.HasValue)
                .WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.Weight)
                .GreaterThan(0)
                .When(x => x.Weight.HasValue)
                .WithMessage("Weight must be greater than 0.");
        }
    }

    public class CreateShipmentChargeValidator : AbstractValidator<CreateShipmentChargeRequest>
    {
        public CreateShipmentChargeValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty().WithMessage("ShipmentId is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(250).WithMessage("Description must not exceed 250 characters.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");
        }
    }

    public class UpdateShipmentChargeValidator : AbstractValidator<UpdateShipmentChargeRequest>
    {
        public UpdateShipmentChargeValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description is required.")
                .MaximumLength(250)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description must not exceed 250 characters.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .When(x => x.Amount.HasValue)
                .WithMessage("Amount must be greater than 0.");
        }
    }

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