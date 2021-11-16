using System.IO;

namespace Hedra.Engine.Management
{
    public class ResourceHandler
    {
        public ResourceHandler(FileStream Stream, string Id)
        {
            this.Stream = Stream;
            this.Id = Id;
        }

        public FileStream Stream { get; }
        public string Id { get; }
        public bool Locked { get; set; }
    }
}