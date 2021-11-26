using FluentValidation;
using MongoDB.Driver;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    readonly DatabaseService _databaseService;
    readonly ApiTokenHelper _apiTokenHelper;

    public UpdateUserCommandValidator(
        ApiTokenHelper apiTokenHelper,
        DatabaseService databaseService)
    {
        _apiTokenHelper = apiTokenHelper;
        _databaseService = databaseService;

        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Token).NotNull().NotEmpty();
        RuleFor(x => x.Token).MustAsync(BeValidApiToken);

        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.Email).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
        RuleFor(x => x.ImageUrl).NotNull().NotEmpty();

        RuleFor(x => x.Gender).Must(x => x == Gender.Male || x == Gender.Female || x == Gender.Female);
        RuleFor(x => x.DateOfBirth).NotNull().Must(x => x != DateTime.MinValue);

        RuleFor(x => x).MustAsync(BeAnExistingUser).WithMessage(x => "User with Id: " + x.Id + "not found.");
        RuleFor(x => x).MustAsync(NotBeAnExistingEmail);
    }
    private Task<bool> BeValidApiToken(string token, CancellationToken arg2)
    {
        return _apiTokenHelper.BeValidApiToken(token);
    }

    private async Task<bool> BeAnExistingUser(UpdateUserCommand command, CancellationToken arg2)
    {
        var exists = await _databaseService.ExistsById<User>(command.Id);
        return exists;
    }

    private async Task<bool> NotBeAnExistingEmail(UpdateUserCommand command, CancellationToken arg2)
    {
        var user = await _databaseService.FindOne<User>(Builders<User>.Filter.Eq(x => x.Email, command.Email));

        // If not data was found then no user exists with this email, so true.
        if (user == null)
            return true;
        // If a user is found but it's id is the same as the the updating user's id then true.
        else if (user != null && user.Id == command.Id)
            return true;
        else
            return false;
    }
}

