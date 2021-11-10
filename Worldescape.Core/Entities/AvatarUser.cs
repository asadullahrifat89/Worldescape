﻿namespace Worldescape.Core;

/// <summary>
/// Information of an avatar's user.
/// </summary>
public class AvatarUser
{
    /// <summary>
    /// User's id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Image Url of the user.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's phone number.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// User's profile picture image url.
    /// </summary>
    public string ProfilePictureUrl { get; set; } = string.Empty;
}

