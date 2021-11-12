using FluentValidation;
using WorldescapeWebService.Core.Declarations.Queries;

namespace WorldescapeWebService.Core.Validators.Queries;

public class GetApiTokenQueryValidator : AbstractValidator<GetApiTokenQuery>
{
    public GetApiTokenQueryValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
    }
}

