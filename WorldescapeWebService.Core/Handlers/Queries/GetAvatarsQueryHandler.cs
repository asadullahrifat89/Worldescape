using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Worldescape.Common;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class GetAvatarsQueryHandler : IRequestHandler<GetAvatarsQuery, GetAvatarsQueryResponse>
{
    #region Fields

    private readonly ILogger<GetAvatarsQueryHandler> _logger;
    private readonly GetAvatarsQueryValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public GetAvatarsQueryHandler(
        ILogger<GetAvatarsQueryHandler> logger,
        GetAvatarsQueryValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<GetAvatarsQueryResponse> Handle(
        GetAvatarsQuery request,
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

            // Get paginated data
            var results = await _databaseService.GetDocuments(filter, skip: request.PageSize * request.PageIndex, limit: request.PageSize);

            return new GetAvatarsQueryResponse()
            {
                Count = count,
                Avatars = results ?? Enumerable.Empty<Avatar>(),
                HttpStatusCode = System.Net.HttpStatusCode.OK,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new GetAvatarsQueryResponse() { HttpStatusCode = System.Net.HttpStatusCode.InternalServerError, Count = 0, Avatars = null, ExternalError = ex.Message };
        }
    }

    #endregion
}

