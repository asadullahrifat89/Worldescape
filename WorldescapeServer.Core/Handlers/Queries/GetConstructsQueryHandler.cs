using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeServer.Core;

public class GetConstructsQueryHandler : IRequestHandler<GetConstructsQuery, RecordsResponse<Construct>>
{
    #region Fields

    private readonly ILogger<GetConstructsQueryHandler> _logger;
    private readonly GetConstructsQueryValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public GetConstructsQueryHandler(
        ILogger<GetConstructsQueryHandler> logger,
        GetConstructsQueryValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<RecordsResponse<Construct>> Handle(
        GetConstructsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            var filter = Builders<Construct>.Filter.Empty;

            filter = Builders<Construct>.Filter.Eq(x => x.World.Id, request.WorldId);

            // Count total number of documents for filter, front end will calculate max number of pages from it
            var count = await _databaseService.CountDocuments(filter);

            // Get paginated data
            var results = await _databaseService.GetDocuments(filter, skip: request.PageSize * request.PageIndex, limit: request.PageSize);

            return new RecordsResponse<Construct>().BuildSuccessResponse(count, results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new RecordsResponse<Construct>().BuildErrorResponse(ex.Message);
        }
    }

    #endregion
}

