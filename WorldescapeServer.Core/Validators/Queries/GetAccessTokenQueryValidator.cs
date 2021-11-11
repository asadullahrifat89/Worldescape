using FluentValidation;

namespace WorldescapeServer.Core;

public class GetAccessTokenQueryValidator : AbstractValidator<GetApiTokenQuery>
{
    public GetAccessTokenQueryValidator()
    {        
        RuleFor(x => x.Email).NotNull().NotEmpty();     
        RuleFor(x => x.Password).NotNull().NotEmpty();
    }
}

