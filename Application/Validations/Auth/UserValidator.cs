using Application.DTOs.Auth;
using FluentValidation;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Identity)
            .NotEmpty().WithMessage("Identity is required.")
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50);

        RuleFor(x => x.UserName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(30)
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Username can only contain letters, numbers, ., _, -");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Matches(@"^\+\d{1,4}$")
            .WithMessage("Country code must be like +20 or +966.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^[0-9]{6,15}$")
            .WithMessage("Phone number must contain only digits.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Matches("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*#?&])[A-Za-z\\d@$!%*#?&]{8,}$")
            .WithMessage("Password must contain letter, number, and special character.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
    public class ConfirmPhoneRequestValidator : AbstractValidator<ConfirmPhoneRequest>
    {
        public ConfirmPhoneRequestValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+\d{7,15}$")
                .WithMessage("Phone number must contain only digits.");

            RuleFor(x => x.Code)
                .MaximumLength(10)
                .NotEmpty().WithMessage("Code is required");
        }
    }
}