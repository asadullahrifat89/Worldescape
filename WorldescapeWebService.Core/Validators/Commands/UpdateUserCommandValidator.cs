using FluentValidation;
using Worldescape.Common;

namespace WorldescapeWebService.Core;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.Email).NotNull().NotEmpty();
        RuleFor(x => x.Phone).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
        RuleFor(x => x.ImageUrl).NotNull().NotEmpty();
        RuleFor(x => x.Gender).Must(x => x == Gender.Male || x == Gender.Female || x == Gender.Female);
        RuleFor(x => x.DateOfBirth).NotNull().Must(x=>x!= DateOnly.MinValue);
    }
}

