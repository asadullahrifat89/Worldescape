using FluentValidation;

namespace WorldescapeWebService.Core;

public class GetAssetQueryValidator : AbstractValidator<GetAssetQuery>
{
    public GetAssetQueryValidator(ApiTokenHelper apiTokenHelper)
    {
        RuleFor(x => x.FileName).NotNull().NotEmpty();
        // RuleFor(x => x.Token).Must(apiTokenHelper.BeValidApiToken);
    }
}

