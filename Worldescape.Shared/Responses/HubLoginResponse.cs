using Worldescape.Shared.Entities;

namespace Worldescape.Shared.Responses
{
    public class HubLoginResponse
    {
        public Avatar[] Avatars { get; set; }

        public Construct[] Constructs { get; set; }
    }
}
