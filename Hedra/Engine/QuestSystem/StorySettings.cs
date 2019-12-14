namespace Hedra.Engine.QuestSystem
{
    public class StorySettings
    {
        public int CompletedStep { get; set; }

        public static StorySettings FromQuest(SerializedQuest Quest)
        {
            return new StorySettings
            {
                CompletedStep = (int)Quest.Content["CompletedStep"]
            };
        }
    }
}