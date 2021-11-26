using FluentValidation;
using MongoDB.Driver;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class UpdateWorldCommandValidator : AbstractValidator<UpdateWorldCommand>
{
    readonly ApiTokenHelper _apiTokenHelper;
    readonly DatabaseService _databaseService;

    public UpdateWorldCommandValidator(
        ApiTokenHelper apiTokenHelper,
        AddWorldCommandValidator validationRules,
        DatabaseService databaseService)
    {
        _databaseService = databaseService;
        _apiTokenHelper = apiTokenHelper;


        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x).MustAsync(BeAnExistingWorld);
        
        RuleFor(x => x).Must(x => validationRules.Validate(x).IsValid);       
    }

    private async Task<bool> BeAnExistingWorld(UpdateWorldCommand command, CancellationToken arg2)
    {
        var exists = await _databaseService.ExistsById<World>(command.Id);
        return exists;
    }
}

