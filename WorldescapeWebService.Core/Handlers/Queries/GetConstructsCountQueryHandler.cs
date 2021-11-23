using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class GetConstructsCountQueryHandler : IRequestHandler<GetConstructsCountQuery, GetConstructsCountQueryResponse>
{
    #region Fields

    private readonly ILogger<GetConstructsCountQueryHandler> _logger;
    private readonly GetConstructsCountQueryValidator _validator;
    private readonly DatabaseService _databaseService;

    #endregion

    #region Ctor

    public GetConstructsCountQueryHandler(
        ILogger<GetConstructsCountQueryHandler> logger,
        GetConstructsCountQueryValidator validator,
        DatabaseService databaseService)
    {
        _logger = logger;
        _validator = validator;
        _databaseService = databaseService;
    }

    #endregion

    #region Methods

    public async Task<GetConstructsCountQueryResponse> Handle(
        GetConstructsCountQuery request,
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

            return new GetConstructsCountQueryResponse() { Count = count, HttpStatusCode = System.Net.HttpStatusCode.OK };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new GetConstructsCountQueryResponse() { Count = 0, HttpStatusCode = System.Net.HttpStatusCode.InternalServerError, ExternalError = ex.Message }; ;
        }
    }

    #endregion
}

