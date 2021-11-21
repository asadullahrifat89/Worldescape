﻿using MediatR;
using Worldescape.Data;

namespace WorldescapeWebService.Core;

public class GetUserQuery : GetUserQueryRequest, IRequest<User>
{
    ///// <summary>
    ///// The email address of the user.
    ///// </summary>
    //public string Email { get; set; } = string.Empty;

    ///// <summary>
    ///// The password of the user.
    ///// </summary>
    //public string Password { get; set; } = string.Empty;
}

