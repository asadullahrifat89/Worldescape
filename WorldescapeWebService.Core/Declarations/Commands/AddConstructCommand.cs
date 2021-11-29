using MediatR;
using Worldescape.Common;

namespace WorldescapeWebService.Core;

/// <summary>
/// A command that inserts a construct.
/// </summary>
public class AddConstructCommand :IRequest<Construct>
{
    public Construct Construct { get; set; }
}

