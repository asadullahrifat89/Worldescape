using FluentValidation;
using WorldescapeWebService.Core.Declarations.Queries;
using WorldescapeWebService.Core.Helpers;

namespace WorldescapeWebService.Core.Validators.Queries;

public class GetWorldsQueryValidator : AbstractValidator<GetWorldsQuery>
{
    public GetWorldsQueryValidator(ApiTokenHelper apiTokenHelper)
    {
        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).Must(apiTokenHelper.BeValidApiToken);

        RuleFor(x => x.PageSize).GreaterThan(0);
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
    }
}

