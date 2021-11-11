﻿using FluentValidation;

namespace WorldescapeWebService.Core;

public class UpdateWorldCommandValidator : AbstractValidator<UpdateWorldCommand>
{
    public UpdateWorldCommandValidator(ApiTokenHelper apiTokenHelper, AddWorldCommandValidator validationRules)
    {
        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).Must(apiTokenHelper.BeValidApiToken);

        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x).Must(x => validationRules.Validate(x).IsValid);
    }
}

