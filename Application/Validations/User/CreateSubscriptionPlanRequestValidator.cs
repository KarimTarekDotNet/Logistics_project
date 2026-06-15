using Application.DTOs.User;
using FluentValidation;

namespace Application.Validations.User
{
    public class CreateSubscriptionPlanRequestValidator : AbstractValidator<CreateSubscriptionPlanRequest>
    {
        public CreateSubscriptionPlanRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(100)
                .MinimumLength(3)
                .Matches(@"^[A-Za-z\s_-]+$")
                .WithMessage("Only letters, spaces, underscore (_) and hyphen (-) are allowed.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .MaximumLength(10)
                .Matches(@"^[A-Z]{3}$")
                .WithMessage("Currency must be a valid 3-letter uppercase code, like USD or EGP.");

            RuleFor(x => x.Description)
               .NotEmpty()
               .MaximumLength(500)
               .MinimumLength(10)
               .Matches(@"^[A-Za-z0-9\s.,_-]+$")
               .WithMessage("Only letters, numbers, spaces, dot, comma, underscore (_) and hyphen (-) are allowed.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");

            RuleFor(x => x.DurationInDays)
                .GreaterThan(0)
                .WithMessage("Duration must be greater than 0 days.")
                .LessThanOrEqualTo(365)
                .WithMessage("Duration must be less than or equal 365 days.");

            RuleFor(x => x.CreateSubscriptionFeatures)
                .NotNull()
                .WithMessage("Features are required.")
                .NotEmpty()
                .WithMessage("At least one feature is required.");

            RuleForEach(x => x.CreateSubscriptionFeatures)
                .SetValidator(new CreateSubscriptionFeatureValidator());

            RuleFor(x => x.CreateSubscriptionPlanLimits)
                .NotNull()
                .WithMessage("Limits are required.")
                .NotEmpty()
                .WithMessage("At least one limit is required.");

            RuleForEach(x => x.CreateSubscriptionPlanLimits)
                .SetValidator(new CreateSubscriptionPlanLimitValidator());
        }
    }

    public class CreateSubscriptionFeatureValidator : AbstractValidator<CreateSubscriptionFeature>
    {
        public CreateSubscriptionFeatureValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(100)
                .Matches(@"^[A-Z][A-Z_-]*$")
                .WithMessage("Only letters, spaces, underscore (_) and hyphen (-) are allowed.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Feature name is required.")
                .MaximumLength(150)
                .WithMessage("Feature name cannot exceed 150 characters.")
                .Matches(@"^[A-Za-z\s_-]+$")
                .WithMessage("Only letters");
        }
    }

    public class CreateSubscriptionPlanLimitValidator : AbstractValidator<CreateSubscriptionPlanLimit>
    {
        public CreateSubscriptionPlanLimitValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(100)
                .Matches(@"^[A-Za-z][A-Za-z0-9_-]*$")
                .WithMessage("Code must contain uppercase letters, numbers, underscore (_) or hyphen (-) only.");

            RuleFor(x => x.MaxValue)
                .GreaterThan(0)
                .WithMessage("Max value must be greater than 0.");
        }
    }
}
