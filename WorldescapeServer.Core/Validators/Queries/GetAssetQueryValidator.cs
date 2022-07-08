using FluentValidation;

namespace WorldescapeServer.Core;

public class GetAssetQueryValidator : AbstractValidator<GetAssetQuery>
{
    private readonly ApiTokenHelper _apiTokenHelper;

    public GetAssetQueryValidator(ApiTokenHelper apiTokenHelper)
    {
        _apiTokenHelper = apiTokenHelper;

        RuleFor(x => x.FileName).NotNull().NotEmpty();

        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).MustAsync(BeValidApiToken);
    }

    private Task<bool> BeValidApiToken(string token, CancellationToken arg2)
    {
        return _apiTokenHelper.BeValidApiToken(token);
    }
}

