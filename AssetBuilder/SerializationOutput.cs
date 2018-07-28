using System.Collections.Generic;

namespace AssetBuilder
{
    public class SerializationOutput
    {
        public Dictionary<string, object> Results { get; set; }
        public BuildHistory History { get; set; }
    }
}