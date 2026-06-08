using Application.DTOs.User;
using FluentValidation;

namespace Application.Validations.User
{
    public class CreateSubscriptionPlanRequestValidator
        : AbstractValidator<CreateSubscriptionPlanRequest>
    {
        public CreateSubscriptionPlanRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MaximumLength(100)
                .WithMessage("Title cannot exceed 100 characters.")
                .MinimumLength(3)
                .WithMessage("Title must be at least 3 characters.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.")
                .MinimumLength(10)
                .WithMessage("Description must be at least 10 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");

            RuleFor(x => x.DurationInDays)
                .GreaterThan(0)
                .WithMessage("Duration must be greater than 0 days.");
        }
    }

    public class UpdateSubscriptionPlanRequestValidator
    : AbstractValidator<UpdateSubscriptionPlanRequest>
    {
        public UpdateSubscriptionPlanRequestValidator()
        {
            RuleFor(x => x.Title)
                .MinimumLength(3)
                .When(x => !string.IsNullOrWhiteSpace(x.Title))
                .WithMessage("Title must be at least 3 characters.")
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Title))
                .WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MinimumLength(10)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description must be at least 10 characters.")
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .When(x => x.Price.HasValue)
                .WithMessage("Price must be greater than 0.");

            RuleFor(x => x.DurationInDays)
                .GreaterThan(0)
                .When(x => x.DurationInDays.HasValue)
                .WithMessage("Duration must be greater than 0 days.");
        }
    }
}
