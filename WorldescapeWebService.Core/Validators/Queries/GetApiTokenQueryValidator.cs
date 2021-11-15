using FluentValidation;

namespace WorldescapeWebService.Core;

public class GetApiTokenQueryValidator : AbstractValidator<GetApiTokenQuery>
{
    public GetApiTokenQueryValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
    }
}

