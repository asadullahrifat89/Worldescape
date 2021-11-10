using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Core;

namespace WorldescapeService.Core;

public class GetAccessTokenQueryHandler : IRequestHandler<GetAccessTokenQuery, StringResponse>
{
    #region Fields

    private readonly ILogger<GetAccessTokenQueryHandler> _logger;
    private readonly GetAccessTokenQueryValidator _validator;

    #endregion

    #region Ctor

    public GetAccessTokenQueryHandler(
        ILogger<GetAccessTokenQueryHandler> logger,
        GetAccessTokenQueryValidator validator)
    {
        _logger = logger;
        _validator = validator;
    }

    #endregion

    #region Methods

    public async Task<StringResponse> Handle(
        GetAccessTokenQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"WorldescapeServiceData.db"))
            {
                // Get Users collection
                var colUsers = db.GetCollection<User>("Users");

                var user = colUsers.FindOne(x => x.Email == request.Email && x.Pasword == request.Password);

                if (user == null)
                    throw new Exception("User not found");

                // Get AccessTokens collection
                var colAccessTokens = db.GetCollection<ApiToken>("AccessTokens");

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

