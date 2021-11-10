using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Core;

namespace WorldescapeService.Core;

public class AddUserCommandHandler : IRequestHandler<AddUserCommand, ServiceResponse>
{
    #region Fields

    private readonly ILogger<AddUserCommandHandler> _logger;
    private readonly AddUserCommandValidator _validator;

    #endregion

    #region Ctor

    public AddUserCommandHandler(
        ILogger<AddUserCommandHandler> logger,
        AddUserCommandValidator validator)
    {
        _logger = logger;
        _validator = validator;
    }

    #endregion

    #region Methods

    public async Task<ServiceResponse> Handle(
        AddUserCommand request,
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

                // Create new user instance
                var user = new User
                {
                    Name = request.Name,
                    ImageUrl = request.ImageUrl,
                    CreatedOn = DateTime.Now,
                    UpdatedOn = null,
                    Email = request.Email,
                    Password = request.Password,
                    Phone = request.Phone,
                };

                // Insert new user document (Id will be auto-incremented)
                BsonValue? userId = colUsers.Insert(user);

                // Get AccessTokens collection
                var colAccessTokens = db.GetCollection<ApiToken>("AccessTokens");

                // Create new access token instance for the saved user
                var accessToken = new ApiToken()
                {
                    UserId = userId.AsInt32,
                    Token = Guid.NewGuid().ToString()
                };

                // Insert new access token document
                colAccessTokens.Upsert(accessToken);
            }

            return new ServiceResponse() { HttpStatusCode = System.Net.HttpStatusCode.OK };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new ServiceResponse() { HttpStatusCode = System.Net.HttpStatusCode.InternalServerError, ExternalError = ex.Message };
        }
    }

    #endregion
}

