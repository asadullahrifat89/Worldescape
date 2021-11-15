namespace WorldescapeWebService.Core;

/// <summary>
/// A query that fetches worlds.
/// </summary>
public class GetWorldsQuery : RequestBase<GetWorldsQueryResponse>
{
    /// <summary>
    /// The page index of the query.
    /// </summary>
    public int PageIndex { get; set; } = 0;

    /// <summary>
    /// The page size of the query.
    /// </summary>
    public int PageSize { get; set; } = 0;

    /// <summary>
    /// The search string of the query.
    /// </summary>
    public string SearchString { get; set; } = string.Empty;
}

