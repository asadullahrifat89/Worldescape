using FluentValidation;

namespace WorldescapeServer.Core;

public class SaveBlobCommandValidator : AbstractValidator<SaveBlobCommand>
{
    readonly ApiTokenHelper _apiTokenHelper;

    public SaveBlobCommandValidator(ApiTokenHelper apiTokenHelper)
    {
        _apiTokenHelper = apiTokenHelper;

        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).MustAsync(BeValidApiToken);

        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.DataUrl).NotNull().NotEmpty();
    }

    private Task<bool> BeValidApiToken(string token, CancellationToken arg2)
    {
        return _apiTokenHelper.BeValidApiToken(token);
    }
}

