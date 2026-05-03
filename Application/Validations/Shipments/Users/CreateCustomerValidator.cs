using Application.DTOs.Shipments.User;
using FluentValidation;

namespace Application.Validations.Shipments.Users
{
    public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
    {
        public CreateCustomerValidator()
        {
            RuleFor(x => x.NationalId)
                .MaximumLength(50)
                .Matches(@"^[A-Za-z0-9\-]+$")
                .When(x => !string.IsNullOrWhiteSpace(x.NationalId))
                .WithMessage("National Id contains invalid characters.");

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
                .When(x => x.DateOfBirth.HasValue)
                .WithMessage("Customer must be at least 18 years old.");

            RuleFor(x => x.CompanyName)
                .MaximumLength(100)
                .Matches(@"^(?!\s)(?!.*\s$)[a-zA-Z0-9\s.,&'@#!?()\[\]\/-]{2,100}$")
                .When(x => !string.IsNullOrWhiteSpace(x.CompanyName))
                .WithMessage("Company Name contains invalid characters.");

            RuleFor(x => x.CountryCode)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.CompanyName) ||
                           !string.IsNullOrWhiteSpace(x.TaxNumber))
                .WithMessage("Country Code is required when Company Name or Tax Number is provided.");

            RuleFor(x => x.CountryCode)
                .Length(2)
                .Matches(@"^[A-Z]{2}$")
                .When(x => !string.IsNullOrWhiteSpace(x.CountryCode))
                .WithMessage("Country Code must be a valid ISO 2-letter code like EG.");

            RuleFor(x => x.TaxNumber)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.CompanyName) ||
                           !string.IsNullOrWhiteSpace(x.CountryCode))
                .WithMessage("Tax Number is required when Company Name or Country Code is provided.");

            RuleFor(x => x.TaxNumber)
                .MaximumLength(50)
                .Matches(@"^[A-Za-z0-9\-]+$")
                .When(x => !string.IsNullOrWhiteSpace(x.TaxNumber))
                .WithMessage("Tax Number contains invalid characters.");

            RuleFor(x => x)
                .Must(x => string.IsNullOrWhiteSpace(x.NationalId) || string.IsNullOrWhiteSpace(x.TaxNumber))
                .WithMessage("Customer cannot have both National Id and Tax Number.");
        }
    }

    public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerRequest>
    {
        public UpdateCustomerValidator()
        {
            RuleFor(x => x.NationalId)
                .MaximumLength(50)
                .Matches(@"^[A-Za-z0-9\-]+$")
                .When(x => !string.IsNullOrWhiteSpace(x.NationalId))
                .WithMessage("National Id contains invalid characters.");

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
                .When(x => x.DateOfBirth.HasValue)
                .WithMessage("Customer must be at least 18 years old.");

            RuleFor(x => x.CompanyName)
                .MaximumLength(100)
                .Matches(@"^(?!\s)(?!.*\s$)[a-zA-Z0-9\s.,&'@#!?()\[\]\/-]{2,100}$")
                .When(x => !string.IsNullOrWhiteSpace(x.CompanyName))
                .WithMessage("Company Name contains invalid characters.");

            RuleFor(x => x.TaxNumber)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.CountryCode))
                .WithMessage("Tax Number is required when Country Code is provided.");

            RuleFor(x => x.TaxNumber)
                .MaximumLength(50)
                .Matches(@"^[A-Za-z0-9\-]+$")
                .When(x => !string.IsNullOrWhiteSpace(x.TaxNumber))
                .WithMessage("Tax Number contains invalid characters.");

            RuleFor(x => x.CountryCode)
                .Length(2)
                .Matches(@"^[A-Z]{2}$")
                .When(x => !string.IsNullOrWhiteSpace(x.CountryCode))
                .WithMessage("Country Code must be a valid ISO 2-letter code like EG.");

            RuleFor(x => x.CountryCode)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.TaxNumber))
                .WithMessage("Country Code is required when Tax Number is provided.");
        }
    }
}