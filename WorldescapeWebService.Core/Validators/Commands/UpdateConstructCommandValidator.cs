using FluentValidation;

namespace WorldescapeWebService.Core;

public class UpdateConstructCommandValidator : AbstractValidator<UpdateConstructCommand>
{
    public UpdateConstructCommandValidator(AddConstructCommandValidator validationRules)
    {
        RuleFor(x => x).Must(x => validationRules.Validate(x).IsValid);
    }
}

