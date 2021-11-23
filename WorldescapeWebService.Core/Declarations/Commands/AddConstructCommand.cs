using MediatR;
using Worldescape.Data;

namespace WorldescapeWebService.Core;

/// <summary>
/// A command that inserts a construct.
/// </summary>
public class AddConstructCommand :IRequest<Construct>
{
    public Construct Construct { get; set; }
}

