using System.IO;

namespace Hedra.Engine.Management
{
    public class ResourceHandler
    {
        public ResourceHandler(FileStream Stream, string Id)
        {
            this.Stream = Stream;
            this.Id = Id;
            Index = new PackedResourceIndex(Stream);
        }

        public FileStream Stream { get; }
        public string Id { get; }
        public bool Locked { get; set; }
        public PackedResourceIndex Index { get; }
    }
}
