﻿using FluentValidation;

namespace WorldescapeServer.Core;

public class AddUserCommandValidator : AbstractValidator<AddUserCommand>
{
    public AddUserCommandValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.Email).NotNull().NotEmpty();
        RuleFor(x => x.Phone).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
        RuleFor(x => x.ImageUrl).NotNull().NotEmpty();
    }
}
