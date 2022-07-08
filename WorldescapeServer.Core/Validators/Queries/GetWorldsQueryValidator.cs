using FluentValidation;

namespace WorldescapeServer.Core;

public class GetWorldsQueryValidator : AbstractValidator<GetWorldsQuery>
{
    private readonly ApiTokenHelper _apiTokenHelper;

    public GetWorldsQueryValidator(ApiTokenHelper apiTokenHelper)
    {
        _apiTokenHelper = apiTokenHelper;

        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).MustAsync(BeValidApiToken);

        RuleFor(x => x.PageSize).GreaterThan(0);
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
    }

    private Task<bool> BeValidApiToken(string token, CancellationToken arg2)
    {
        return _apiTokenHelper.BeValidApiToken(token);
    }
}

