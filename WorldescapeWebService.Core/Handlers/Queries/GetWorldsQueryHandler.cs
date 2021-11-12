using LiteDB;
using MediatR;
using Microsoft.Extensions.Logging;
using Worldescape.Common.Entities;
using WorldescapeWebService.Core.Declarations.Queries;
using WorldescapeWebService.Core.Extensions;
using WorldescapeWebService.Core.Responses.Queries;
using WorldescapeWebService.Core.Validators.Queries;

namespace WorldescapeWebService.Core.Handlers.Queries;

public class GetWorldsQueryHandler : IRequestHandler<GetWorldsQuery, GetWorldsQueryResponse>
{
    #region Fields

    private readonly ILogger<GetWorldsQueryHandler> _logger;
    private readonly GetWorldsQueryValidator _validator;

    #endregion

    #region Ctor

    public GetWorldsQueryHandler(
        ILogger<GetWorldsQueryHandler> logger,
        GetWorldsQueryValidator validator)
    {
        _logger = logger;
        _validator = validator;
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

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Worldescape.db"))
            {
                // Get Worlds collection
                var colWorlds = db.GetCollection<World>("Worlds");

                var results = colWorlds.Find(
                    predicate: string.IsNullOrEmpty(request.SearchString) || string.IsNullOrWhiteSpace(request.SearchString) ? x => x.Id > 0 : x => x.Name.ToLower().Contains(request.SearchString.ToLower()),
                    skip: request.PageSize * request.PageIndex,
                    limit: request.PageSize);

                return new GetWorldsQueryResponse() { Count = results != null ? results.Count() : 0, Worlds = results ?? Enumerable.Empty<World>() };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new GetWorldsQueryResponse();
        }
    }

    #endregion
}

