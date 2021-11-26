using FluentValidation;
using MongoDB.Driver;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class AddUserCommandValidator : AbstractValidator<AddUserCommand>
{
    readonly DatabaseService _databaseService;

    public AddUserCommandValidator(DatabaseService databaseService)
    {
        _databaseService = databaseService;

        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.Email).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
        //RuleFor(x => x.Phone).NotNull().NotEmpty();
        //RuleFor(x => x.ImageUrl).NotNull().NotEmpty();

        RuleFor(x => x.Gender).Must(x => x == Gender.Male || x == Gender.Female || x == Gender.Female);
        RuleFor(x => x.DateOfBirth).NotNull().Must(x => x != DateTime.MinValue);

        RuleFor(x => x).MustAsync(NotBeAnExistingEmail).WithMessage(x => "User with Email: " + x.Email + " already exists.");
    }

    private async Task<bool> NotBeAnExistingEmail(AddUserCommand command, CancellationToken arg2)
    {
        var exists = await _databaseService.Exists<User>(Builders<User>.Filter.Eq(x => x.Email, command.Email));
        return !exists;
    }
}

