using FluentValidation;

namespace WorldescapeWebService.Core;

public class AddWorldCommandValidator : AbstractValidator<AddWorldCommand>
{
    private readonly ApiTokenHelper _apiTokenHelper;

    public AddWorldCommandValidator(ApiTokenHelper apiTokenHelper)
    {
        _apiTokenHelper = apiTokenHelper;

        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).MustAsync(BeValidApiToken);

        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.ImageUrl).NotNull().NotEmpty();
    }

    private Task<bool> BeValidApiToken(string token, CancellationToken arg2)
    {
        return _apiTokenHelper.BeValidApiToken(token);
    }
}

