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
        }
    }
}