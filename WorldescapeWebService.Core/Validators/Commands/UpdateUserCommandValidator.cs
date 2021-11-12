using FluentValidation;
using WorldescapeWebService.Core.Declarations.Commands;

namespace WorldescapeWebService.Core.Validators.Commands;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(AddUserCommandValidator validationRules)
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x).Must(x => validationRules.Validate(x).IsValid);
    }
}

