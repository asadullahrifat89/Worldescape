using static Worldescape.Common.Enums;

namespace Worldescape.Common;

/// <summary>
/// An image url bound to an activity status.
/// </summary>
public class StatusBoundImageUrl
{
    /// <summary>
    /// An activity status.
    /// </summary>
    public ActivityStatus Status { get; set; }

    /// <summary>
    /// Image url for the activity status.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
}

