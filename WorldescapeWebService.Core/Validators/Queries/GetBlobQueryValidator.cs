using FluentValidation;

namespace WorldescapeWebService.Core;

public class GetBlobQueryValidator : AbstractValidator<GetBlobQuery>
{
    private readonly ApiTokenHelper _apiTokenHelper;

    public GetBlobQueryValidator(ApiTokenHelper apiTokenHelper)
    {
        _apiTokenHelper = apiTokenHelper;

        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).MustAsync(BeValidApiToken);
    }

    private Task<bool> BeValidApiToken(string token, CancellationToken arg2)
    {
        return _apiTokenHelper.BeValidApiToken(token);
    }
}

