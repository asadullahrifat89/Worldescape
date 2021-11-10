using FluentValidation;

namespace WorldescapeService.Core;

public class GetAccessTokenQueryValidator : AbstractValidator<GetAccessTokenQuery>
{
    public GetAccessTokenQueryValidator()
    {        
        RuleFor(x => x.Email).NotNull().NotEmpty();     
        RuleFor(x => x.Password).NotNull().NotEmpty();
    }
}

