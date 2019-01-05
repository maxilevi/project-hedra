using System;
using Hedra.Components;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem.Designs.Auxiliaries;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public abstract class QuestDesign
    {
        private bool IsAuxiliary => Tier == QuestTier.Auxiliary;

        protected virtual bool HasNext => true;

        protected abstract QuestReward BuildReward(Random Rng);

        public QuestReward GetReward(QuestObject Quest)
        {
            return BuildReward(new Random(Quest.Seed + 42));
        }
        
        public abstract QuestTier Tier { get; }

        public abstract string GetThoughtsKeyword(QuestObject Quest);

        public abstract string Name { get; }

        public abstract object[] GetThoughtsParameters(QuestObject Quest);
        
        public abstract string GetShortDescription(QuestObject Quest);

        public abstract string GetDescription(QuestObject Quest);

        public abstract QuestView BuildView(QuestObject Quest);
        
        protected virtual QuestObject Setup(QuestObject Quest)
        {
            return Quest;
        }

        protected virtual QuestParameters BuildParameters(QuestObject Previous, QuestContext Context, QuestParameters Parameters, Random Rng)
        {
            return Parameters;
        }

        protected void AddDialogLine(QuestObject Quest, Translation Text)
        {
            Quest.Giver.SearchComponent<TalkComponent>().AddDialogLine(Text);
        }

        protected abstract QuestDesign[] Auxiliaries { get; }

        protected abstract QuestDesign[] Descendants { get; }

        public abstract bool IsQuestCompleted(QuestObject Quest);
        
        protected abstract void Consume(QuestObject Quest);
       
        public virtual void Abandon(QuestObject Quest)
        {
        }
        
        public void Trigger(QuestObject Object)
        {
            Consume(Object);
            if (Object.Giver.SearchComponent<QuestComponent>() != null)
                Object.Giver.RemoveComponent(Object.Giver.SearchComponent<QuestComponent>());
            if(HasNext)
                Object.Owner.Questing.Start(GetNext(Object));
        }

        public QuestObject GetNext(QuestObject Quest)
        {
            if (!IsAuxiliary)
            {
                var rng = new Random(Quest.Parameters.Get<int>("Seed"));
                var nextDesign = Descendants?[rng.Next(0, Descendants.Length)];
                var auxiliaryDesign = nextDesign != null 
                    ? Auxiliaries[rng.Next(0, Auxiliaries.Length)]
                    : new EndDesign();
                var nextObject = nextDesign?.Build(Quest, null, null);
                var questObject = auxiliaryDesign.Build(Quest, nextDesign, nextObject);
                questObject.Parameters.Set("Next", nextDesign);
                questObject.Parameters.Set("NextObject", nextObject);

                return questObject;
            }
            return Quest.Parameters.Get<QuestObject>("NextObject");
        }

        public bool IsEndQuest(QuestObject Quest)
        {
            var rng = new Random(Quest.Parameters.Get<int>("Seed"));
            return Descendants?[rng.Next(0, Descendants.Length)] == null;
        }
        
        public QuestObject Build(Vector3 Position, Random Rng, IHumanoid Giver)
        {
            return Build(new QuestContext(Position), Rng.Next(int.MinValue, int.MaxValue), Giver);
        }

        private QuestObject Build(QuestObject Object, QuestDesign NextDesign, QuestObject NextObject)
        {
            return Build(
                Object.Parameters.Get<QuestContext>("Context"),
                Object.Parameters.Get<int>("Seed"),
                Object.Giver,
                Object.BaseDesign,
                Object.Steps + 1,
                Object,
                NextDesign,
                NextObject
            );
        }

        public QuestObject Build(
            QuestContext Context,
            int Seed,
            IHumanoid Giver,
            QuestDesign BaseDesign = null,
            int Steps = 0,
            QuestObject Previous = null,
            QuestDesign NextDesign = null,
            QuestObject NextObject = null
        )
        {
            var parameters = new QuestParameters();
            parameters.Set("Seed", Seed);
            parameters.Set("Context", Context);
            parameters.Set("Next", NextDesign);
            parameters.Set("NextObject", NextObject);
            return Setup(new QuestObject(
                this,
                BuildParameters(Previous, Context, parameters, new Random(parameters.Get<int>("Seed"))),
                Giver,
                BaseDesign ?? this,
                Steps
            ));
        }
    }
}