using FluentValidation;

namespace WorldescapeWebService.Core;

public class AddWorldCommandValidator : AbstractValidator<AddWorldCommand>
{
    public AddWorldCommandValidator(ApiTokenHelper apiTokenHelper)
    {
        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).Must(apiTokenHelper.BeValidApiToken);

        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.ImageUrl).NotNull().NotEmpty();
    }
}

