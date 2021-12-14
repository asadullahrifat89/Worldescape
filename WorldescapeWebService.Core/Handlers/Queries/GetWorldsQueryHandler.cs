using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class GetWorldsQueryHandler : IRequestHandler<GetWorldsQuery, RecordsResponse<World>>
{
    #region Fields

    private readonly ILogger<GetWorldsQueryHandler> _logger;
    private readonly GetWorldsQueryValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public GetWorldsQueryHandler(
        ILogger<GetWorldsQueryHandler> logger,
        GetWorldsQueryValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<RecordsResponse<World>> Handle(
        GetWorldsQuery request,
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

            // Get paginated data
            var results = await _databaseService.GetDocuments(filter, skip: request.PageSize * request.PageIndex, limit: request.PageSize);

            return new RecordsResponse<World>().BuildSuccessResponse(count, results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new RecordsResponse<World>().BuildErrorResponse(ex.Message);
        }
    }

    #endregion
}

