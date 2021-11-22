using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
{
    #region Fields

    private readonly ILogger<GetUserQueryHandler> _logger;
    private readonly GetUserQueryValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public GetUserQueryHandler(
        ILogger<GetUserQueryHandler> logger,
        GetUserQueryValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<User> Handle(
        GetUserQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            // Find the token
            var accessToken = await _databaseService.FindOne(Builders<ApiToken>.Filter.Eq(x => x.Token, request.Token));

            if (accessToken == null)
                throw new Exception("Token expired");

            var user = await _databaseService.FindById<User>(accessToken.UserId);

            if (user == null || user.IsEmpty())
                throw new Exception("User not found");

            if (user.Email != request.Email)
                throw new Exception("Invalid email");

            if (user.Password != request.Password)
                throw new Exception("Invalid password");

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new User();
        }
    }

    #endregion
}

