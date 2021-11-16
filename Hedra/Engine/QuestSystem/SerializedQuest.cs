using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Hedra.Engine.ModuleSystem;

namespace Hedra.Engine.QuestSystem
{
    public class SerializedQuest : SerializableTemplate<SerializedQuest>
    {
        public string Name { get; set; }
        public string GiverName { get; set; }
        public Vector3 GivenPosition { get; set; }
        public Dictionary<string, object> Content { get; set; }
        public bool IsMetadata { get; set; }

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

        public static SerializedQuest FromStoryline(StorySettings Settings)
        {
            var quest = new SerializedQuest
            {
                Content = new Dictionary<string, object>
                {
                    { "CompletedSteps", Settings.CompletedSteps }
                },
                Name = "Story",
                IsMetadata = true
            };
            return quest;
        }
    }
}