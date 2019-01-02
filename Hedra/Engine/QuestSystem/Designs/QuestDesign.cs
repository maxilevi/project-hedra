using System;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Designs
{
    public abstract class QuestDesign
    {
        private bool IsAuxiliary => Tier == QuestTier.Auxiliary;
        
        public abstract QuestTier Tier { get; }
        
        public abstract string ThoughtsKeyword { get; }

        public abstract string Name { get; }

        public abstract object[] GetThoughtsParameters(QuestObject Quest);
        
        public abstract string GetShortDescription(QuestObject Quest);

        public abstract string GetDescription(QuestObject Quest);

        public abstract QuestView BuildView(QuestObject Quest);
        
        protected virtual QuestObject Setup(QuestObject Object)
        {
            return Object;
        }

        protected virtual QuestParameters BuildParameters(QuestContext Context, QuestParameters Parameters, Random Rng)
        {
            return Parameters;
        }

        protected abstract QuestDesign[] Auxiliaries { get; }

        protected abstract QuestDesign[] Descendants { get; }

        public abstract bool IsQuestCompleted(QuestObject Object);
        
        protected abstract void Consume(QuestObject Object);
       
        public virtual void Abandon(QuestObject Object)
        {
        }
        
        public void Trigger(QuestObject Object)
        {
            Consume(Object);
            Object.Owner.Questing.Start(GetNext(Object));
        }

        public QuestObject GetNext(QuestObject Object)
        {
            if (!IsAuxiliary)
            {
                var rng = new Random(Object.Parameters.Get<int>("Seed"));
                var nextDesign = Descendants?[rng.Next(0, Descendants.Length)];
                var auxiliaryDesign = Auxiliaries[rng.Next(0, Auxiliaries.Length)];
                var questObject = auxiliaryDesign.Build(Object, nextDesign);
                if(nextDesign != null)
                    questObject.Parameters.Set("Next", nextDesign);
                return questObject;
            }
            return Object.Parameters.Get<QuestDesign>("Next")?.Build(Object);
        }
        
        public QuestObject Build(Vector3 Position, Random Rng, IHumanoid Giver)
        {
            return Build(new QuestContext(Position), Rng.Next(int.MinValue, int.MaxValue), Giver);
        }

        private QuestObject Build(QuestObject Object, QuestDesign NextDesign = null)
        {
            return Build(
                Object.Parameters.Get<QuestContext>("Context"),
                Object.Parameters.Get<int>("Seed"),
                Object.Giver,
                Object.BaseDesign,
                Object.Steps + 1,
                NextDesign
            );
        }

        public QuestObject Build(
            QuestContext Context,
            int Seed,
            IHumanoid Giver,
            QuestDesign BaseDesign = null,
            int Steps = 0,
            QuestDesign NextDesign = null
        )
        {
            var parameters = new QuestParameters();
            parameters.Set("Seed", Seed);
            parameters.Set("Context", Context);
            if(NextDesign != null) parameters.Set("Next", NextDesign);
            return Setup(new QuestObject(
                this,
                BuildParameters(Context, parameters, new Random(parameters.Get<int>("Seed"))),
                Giver,
                BaseDesign ?? this,
                Steps
            ));
        }
    }
}