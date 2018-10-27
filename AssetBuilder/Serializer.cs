using System.Collections.Generic;

namespace AssetBuilder
{
    public abstract class Serializer
    {
        public abstract SerializationOutput Serialize(string[] Files);
    }
}
