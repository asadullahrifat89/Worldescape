using FluentValidation;

namespace WorldescapeServer.Core;

public class GetAvatarsCountQueryValidator : AbstractValidator<GetAvatarsCountQuery>
{
    private readonly ApiTokenHelper _apiTokenHelper;

    public GetAvatarsCountQueryValidator(ApiTokenHelper apiTokenHelper)
    {
        _apiTokenHelper = apiTokenHelper;

        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).MustAsync(BeValidApiToken);

        RuleFor(x => x.WorldId).GreaterThan(0);
    }

    private Task<bool> BeValidApiToken(string token, CancellationToken arg2)
    {
        return _apiTokenHelper.BeValidApiToken(token);
    }
}

