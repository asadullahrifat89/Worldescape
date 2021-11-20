using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Worldescape.Data;
using Worldescape.Database;

namespace WorldescapeWebService.Core;

public class GetWorldsQueryHandler : IRequestHandler<GetWorldsQuery, GetWorldsQueryResponse>
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

    public async Task<GetWorldsQueryResponse> Handle(
        GetWorldsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            validationResult.EnsureValidResult();

            var filter = Builders<World>.Filter.Empty;

            if (!string.IsNullOrEmpty(request.SearchString) && !string.IsNullOrWhiteSpace(request.SearchString))
                filter = Builders<World>.Filter.AnyIn(x => x.Name, request.SearchString);

            var results = await _databaseService.GetDocuments(filter, skip: request.PageSize * request.PageIndex, limit: request.PageSize);

            return new GetWorldsQueryResponse() { Count = results != null ? results.Count() : 0, Worlds = results ?? Enumerable.Empty<World>() };

            //// Open database (or create if doesn't exist)
            //using (var db = new LiteDatabase(@"Worldescape.db"))
            //{
            //    // Get Worlds collection
            //    var colWorlds = db.GetCollection<World>("Worlds");

            //    var results = colWorlds.Find(
            //        predicate: string.IsNullOrEmpty(request.SearchString) || string.IsNullOrWhiteSpace(request.SearchString) ? x => x.Id > 0 : x => x.Name.ToLower().Contains(request.SearchString.ToLower()),
            //        skip: request.PageSize * request.PageIndex,
            //        limit: request.PageSize);

            //    return new GetWorldsQueryResponse() { Count = results != null ? results.Count() : 0, Worlds = results ?? Enumerable.Empty<World>() };
            //}
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new GetWorldsQueryResponse();
        }
    }

    #endregion
}

