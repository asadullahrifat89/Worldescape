﻿using MediatR;

namespace WorldescapeServer.Core
{
    public class RequestBase<T> : IRequest<T>
    {
        /// <summary>
        /// The token for authentication.
        /// </summary>
        public string Token { get; set; }
    }
}
