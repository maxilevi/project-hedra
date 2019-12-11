using System.Collections.Generic;
using System.Text;
using Hedra.Engine.ModuleSystem;
using System.Numerics;
using Hedra.Engine.CacheSystem;

namespace Hedra.Engine.QuestSystem
{
    public class SerializedQuest : SerializableTemplate<SerializedQuest>
    {
        public string Name { get; set; }
        public string GiverName { get; set; }
        public Vector3 GivenPosition { get; set; }

        public byte[] ToArray()
        {
            return Encoding.ASCII.GetBytes(
                ToJson(this)
            );
        }
        
        public static SerializedQuest FromArray(byte[] Array)
        {
            var str = Encoding.ASCII.GetString(Array);
            return FromJson(str);
        }
    }
}