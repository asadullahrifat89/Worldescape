namespace Worldescape.Common
{
    /// <summary>
    /// A command that saves a blob.
    /// </summary>
    public class SaveBlobCommandRequest : RequestBase
    {
        public int Id { get; set; }

        public string DataUrl { get; set; } = string.Empty;
    }
}



