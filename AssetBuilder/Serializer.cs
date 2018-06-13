using System.Collections.Generic;

namespace AssetBuilder
{
    public abstract class Serializer
    {
        public abstract Dictionary<string, object> Serialize(string[] Files);
    }
}
