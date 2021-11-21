using MediatR;

namespace WorldescapeWebService.Core;

/// <summary>
/// A command that inserts or updates a user.
/// </summary>
public class GetAssetQuery : RequestBase<byte[]>
{
    /// <summary>
    /// The name of the file to be fetched.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

