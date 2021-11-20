using FluentValidation;

namespace WorldescapeWebService.Core;

public class UpdateWorldCommandValidator : AbstractValidator<UpdateWorldCommand>
{
    private readonly ApiTokenHelper _apiTokenHelper;

    public UpdateWorldCommandValidator(ApiTokenHelper apiTokenHelper, AddWorldCommandValidator validationRules)
    {
        _apiTokenHelper = apiTokenHelper;

        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).MustAsync(BeValidApiToken);

        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x).Must(x => validationRules.Validate(x).IsValid);
    }

    private Task<bool> BeValidApiToken(string token, CancellationToken arg2)
    {
        return _apiTokenHelper.BeValidApiToken(token);
    }
}

