using FluentValidation;

namespace WorldescapeServer.Core;

public class GetApiTokenQueryValidator : AbstractValidator<GetApiTokenQuery>
{
    public GetApiTokenQueryValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
    }
}

