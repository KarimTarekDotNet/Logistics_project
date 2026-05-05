using Application.DTOs.User;
using FluentValidation;

namespace Application.Validations.User
{
    public class UpdateUserValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Invalid email format.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Invalid phone number format. It should be in E.164 format.");

            RuleFor(x => x.Username)
                .Matches(@"^[a-zA-Z0-9_]+$")
                .MinimumLength(3)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.Username))
                .WithMessage("Username must be between 3 and 20 characters.");

            RuleFor(x => x.FirstName)
                .MinimumLength(3)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.FirstName))
                .WithMessage("First name must be between 3 and 50 characters.");

            RuleFor(x => x.LastName)
                .MinimumLength(3)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.LastName))
                .WithMessage("Last name must be between 3 and 50 characters.");
        }
    }

    public class UpdatePasswordValidator : AbstractValidator<UpdatePasswordRequest>
    {
        public UpdatePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$")
                .NotEmpty()
                .MinimumLength(6)
                .WithMessage("New password must be at least 6 characters long.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("Confirm password must match the new password.");
        }
    }
}
