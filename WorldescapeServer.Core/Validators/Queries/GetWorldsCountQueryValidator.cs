using FluentValidation;

namespace WorldescapeServer.Core;

public class GetWorldsCountQueryValidator : AbstractValidator<GetWorldsCountQuery>
{
    private readonly ApiTokenHelper _apiTokenHelper;

    public GetWorldsCountQueryValidator(ApiTokenHelper apiTokenHelper)
    {
        _apiTokenHelper = apiTokenHelper;

        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).MustAsync(BeValidApiToken);
    }

    private Task<bool> BeValidApiToken(string token, CancellationToken arg2)
    {
        return _apiTokenHelper.BeValidApiToken(token);
    }
}

