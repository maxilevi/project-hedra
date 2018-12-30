using Hedra.Engine.ModuleSystem;

namespace Hedra.Engine.QuestSystem
{
    public class QuestTemplate : SerializableTemplate<QuestTemplate>
    {
        public string Name { get; set; }
        public int Seed { get; set; }
        public QuestContextType Context { get; set; }

        public static QuestTemplate FromQuest(QuestObject Object)
        {
            return new QuestTemplate
            {
                Name = Object.Name,
                Seed = Object.Parameters.Get<int>("Seed"),
                Context = Object.Parameters.Get<QuestContext>("Context").ContextType
            };
        }
    }
}