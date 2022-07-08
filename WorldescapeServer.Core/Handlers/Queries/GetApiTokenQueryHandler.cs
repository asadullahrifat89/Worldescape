using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeServer.Core;

public class GetApiTokenQueryHandler : IRequestHandler<GetApiTokenQuery, RecordResponse<string>>
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

    public async Task<RecordResponse<string>> Handle(
        GetApiTokenQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            var userfilter = Builders<User>.Filter.Eq(x => x.Email, request.Email);

            var user = await _databaseService.FindOne(userfilter);

            if (user == null || user.IsEmpty())
                throw new Exception("User not found");

            if (user.Password != request.Password)
                throw new Exception("Invalid password");

            var tokenfilter = Builders<ApiToken>.Filter.Eq(x => x.UserId, user.Id);
            var accessToken = await _databaseService.FindOne(tokenfilter);

            return new RecordResponse<string>().BuildSuccessResponse(accessToken.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new RecordResponse<string>().BuildErrorResponse(ex.Message);
        }
    }

    #endregion
}

