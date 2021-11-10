namespace Worldescape.Core;

public class ApiToken
{
    /// <summary>
    /// Id of the user to which this token is being generated.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The token generated for the user.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}

