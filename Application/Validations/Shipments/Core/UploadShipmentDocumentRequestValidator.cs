using Application.DTOs.Shipments.Core;
using Domain.Enums;
using FluentValidation;

namespace Application.Validators.Shipments.Core
{
    public class UploadShipmentDocumentRequestValidator
        : AbstractValidator<UploadShipmentDocumentRequest>
    {
        public UploadShipmentDocumentRequestValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid document type.");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File is required.");

            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .When(x => x.File != null)
                .WithMessage("File cannot be empty.")
                .LessThanOrEqualTo(5 * 1024 * 1024)
                .When(x => x.File != null)
                .WithMessage("File size must not exceed 5 MB.");

            RuleFor(x => x.File.FileName)
                .NotEmpty()
                .When(x => x.File != null)
                .WithMessage("File name is required.");
        }

        private static bool BeValidDocumentType(string type)
        {
            return Enum.TryParse<DocumentType>(
                type,
                ignoreCase: true,
                out _);
        }
    }
}