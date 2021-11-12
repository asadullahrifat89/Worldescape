using FluentValidation;
using WorldescapeWebService.Core.Declarations.Commands;
using WorldescapeWebService.Core.Helpers;

namespace WorldescapeWebService.Core.Validators.Commands;

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

