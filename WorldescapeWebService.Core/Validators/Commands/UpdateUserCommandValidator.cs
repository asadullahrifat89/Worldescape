using FluentValidation;

namespace WorldescapeWebService.Core;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(AddUserCommandValidator validationRules)
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ImageUrl).NotNull().NotEmpty();
        RuleFor(x => x).Must(x => validationRules.Validate(x).IsValid);
    }
}

