using System;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;

namespace Hedra.Engine.QuestSystem
{
    public abstract class QuestDesign 
    {
        public abstract QuestTier Tier { get; }
        
        public abstract Translation OpeningLine { get; }
        
        public abstract QuestView View { get; }

        protected abstract QuestParameters BuildParameters(QuestContext Context, QuestParameters Parameters, Random Rng);
        
        public abstract QuestDesign[] Predecessors { get; }
        
        public abstract QuestDesign[] Descendants { get; }

        public abstract string ToString(QuestObject Object);

        public abstract bool IsQuestCompleted(QuestObject Object, IPlayer Player);

        public QuestObject Build()
        {
            return new QuestObject();
        }
    }
}