using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class GetAvatarsCountQueryHandler : IRequestHandler<GetAvatarsCountQuery, RecordsCountResponse>
{
    #region Fields

    private readonly ILogger<GetAvatarsCountQueryHandler> _logger;
    private readonly GetAvatarsCountQueryValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public GetAvatarsCountQueryHandler(
        ILogger<GetAvatarsCountQueryHandler> logger,
        GetAvatarsCountQueryValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<RecordsCountResponse> Handle(
        GetAvatarsCountQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            var filter = Builders<Avatar>.Filter.Empty;

            filter = Builders<Avatar>.Filter.Eq(x => x.World.Id, request.WorldId);

            // Count total number of documents for filter, front end will calculate max number of pages from it
            var count = await _databaseService.CountDocuments(filter);

            return new RecordsCountResponse().BuildSuccessResponse(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new RecordsCountResponse().BuildErrorResponse(ex.Message);
        }
    }

    #endregion
}

