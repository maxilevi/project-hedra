using System;

namespace Hedra.Engine.QuestSystem
{
    public class StorySettings
    {
        public int CompletedSteps { get; set; }

        public static StorySettings FromQuest(SerializedQuest Quest)
        {
            return new StorySettings
            {
                CompletedSteps = Convert.ToInt32(Quest.Content["CompletedSteps"])
            };
        }
    }
}