using System.Text;
using Hedra.Engine.ModuleSystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public class QuestTemplate : SerializableTemplate<QuestTemplate>
    {
        public string Name { get; set; }
        public int Seed { get; set; }
        public int Steps { get; set; }
        public QuestContextType Context { get; set; }
        public GiverTemplate Giver { get; set; }
          
        public byte[] ToArray()
        {
            return Encoding.ASCII.GetBytes(
                ToJson(this)
            );
        }
        
        public static QuestTemplate FromArray(byte[] Array)
        {
            var str = Encoding.ASCII.GetString(Array);
            return FromJSON(str);
        }
    }
}