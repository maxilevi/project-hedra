using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public abstract class QuestDesign 
    {
        public abstract QuestTier Tier { get; }

        public abstract string Name { get; }
        
        public abstract string GetDisplayName(QuestObject Quest);

        public abstract string GetDescription(QuestObject Quest);
        
        public abstract QuestView View { get; }

        protected abstract QuestParameters BuildParameters(QuestContext Context, QuestParameters Parameters, Random Rng);
        
        public abstract QuestDesign[] Predecessors { get; }
        
        public abstract QuestDesign[] Descendants { get; }

        public abstract string ToString(QuestObject Object);

        public abstract bool IsQuestCompleted(QuestObject Object, IPlayer Player);

        public abstract VertexData BuildPreview(QuestObject Object);
        
        public abstract VertexData Icon { get; }
        
        public QuestObject Build(Vector3 Position, Random Rng)
        {
            return Build(new QuestContext(Position), Rng.Next(int.MinValue, int.MaxValue));
        }

        public QuestObject Build(QuestContext Context, int Seed)
        {
            var parameters = new QuestParameters();
            parameters.Set("Seed", Seed);
            parameters.Set("Context", Context);
            return new QuestObject(
                this,
                BuildParameters(Context, parameters, new Random(parameters.Get<int>("Seed")))
            );
        }
    }
}