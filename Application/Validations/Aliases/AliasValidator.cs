using Application.DTOs.Aliases;
using FluentValidation;

namespace Application.Validations.Aliases
{
    public class CreateAliasRequestValidator : AbstractValidator<CreateAliasRequest>
    {
        public CreateAliasRequestValidator()
        {
            RuleFor(x => x.AliasName)
                .NotEmpty()
                .MaximumLength(150)
                .Matches(@"^[\p{L}0-9\s\-\._/()&]+$")
                .WithMessage("Alias contains invalid characters.");

            RuleFor(x => x.Type)
                .IsInEnum();

            RuleFor(x => x.EntityId)
                .NotEmpty();
        }
    }
    public class UpdateAliasRequestValidator : AbstractValidator<UpdateAliasRequest>
    {
        public UpdateAliasRequestValidator()
        {
            RuleFor(x => x.AliasName)
            .MaximumLength(150)
            .Matches(@"^[\p{L}0-9\s\-\._/()&]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.AliasName))
            .WithMessage("Alias contains invalid characters.");
        }
    }
}
