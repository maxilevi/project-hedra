using System.IO;

namespace Hedra.Engine.Management
{
    public class ResourceHandler
    {
        public FileStream Stream { get; }
        public string Id { get; }
        public bool Locked { get; set; }

        public ResourceHandler(FileStream Stream, string Id)
        {
            this.Stream = Stream;
            this.Id = Id;
        }
    }
}