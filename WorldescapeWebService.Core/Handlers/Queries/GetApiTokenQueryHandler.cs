using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class GetApiTokenQueryHandler : IRequestHandler<GetApiTokenQuery, StringResponse>
{
    #region Fields

    private readonly ILogger<GetApiTokenQueryHandler> _logger;
    private readonly GetApiTokenQueryValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public GetApiTokenQueryHandler(
        ILogger<GetApiTokenQueryHandler> logger,
        GetApiTokenQueryValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<StringResponse> Handle(
        GetApiTokenQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get Users collection
                var colUsers = db.GetCollection<User>("Users");

                var user = colUsers.FindOne(x => x.Email == request.Email);

                if (user == null || user.IsEmpty())
                    throw new Exception("User not found");

                if (user.Password != request.Password)
                    throw new Exception("Invalid password");

                // Get AccessTokens collection
                var colAccessTokens = db.GetCollection<ApiToken>("ApiTokens");

                var accessToken = colAccessTokens.FindOne(x => x.UserId == user.Id);

                return new StringResponse() { Response = accessToken.Token };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new StringResponse() { HttpStatusCode = System.Net.HttpStatusCode.InternalServerError, ExternalError = ex.Message };
        }
    }

    #endregion
}

