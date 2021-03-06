using FluentValidation;

namespace WorldescapeWebService.Core;

public class GetConstructsCountQueryValidator : AbstractValidator<GetConstructsCountQuery>
{
    private readonly ApiTokenHelper _apiTokenHelper;

    public GetConstructsCountQueryValidator(ApiTokenHelper apiTokenHelper)
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

