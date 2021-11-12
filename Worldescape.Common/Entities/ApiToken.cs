namespace Worldescape.App.Core;

public class ApiToken
{
    /// <summary>
    /// Id of the token.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Id of the user to which this token is being generated.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The token generated for the user.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}

