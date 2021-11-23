﻿using FluentValidation;

namespace WorldescapeWebService.Core;

public class GetConstructsQueryValidator : AbstractValidator<GetConstructsQuery>
{
    private readonly ApiTokenHelper _apiTokenHelper;

    public GetConstructsQueryValidator(ApiTokenHelper apiTokenHelper)
    {
        _apiTokenHelper = apiTokenHelper;

        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).MustAsync(BeValidApiToken);

        RuleFor(x => x.PageSize).GreaterThan(0);
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);

        RuleFor(x => x.WorldId).GreaterThan(0);
    }

    private Task<bool> BeValidApiToken(string token, CancellationToken arg2)
    {
        return _apiTokenHelper.BeValidApiToken(token);
    }
}

