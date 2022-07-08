using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeServer.Core;

public class GetWorldsCountQueryHandler : IRequestHandler<GetWorldsCountQuery, RecordsCountResponse>
{
    #region Fields

    private readonly ILogger<GetWorldsCountQueryHandler> _logger;
    private readonly GetWorldsCountQueryValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public GetWorldsCountQueryHandler(
        ILogger<GetWorldsCountQueryHandler> logger,
        GetWorldsCountQueryValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<RecordsCountResponse> Handle(
        GetWorldsCountQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            var filter = Builders<World>.Filter.Empty;

            if (!request.SearchString.IsNullOrBlank())
                filter &= Builders<World>.Filter.Regex(x => x.Name, new BsonRegularExpression("/.*" + request.SearchString + ".*/i"));

            if (request.CreatorId > 0)
                filter &= Builders<World>.Filter.Eq(x => x.Creator.Id, request.CreatorId);

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

