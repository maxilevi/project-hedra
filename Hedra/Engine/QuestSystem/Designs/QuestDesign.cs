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

        /// <summary>
        /// Builds a quest reward for this design.
        /// </summary>
        /// <param name="Rng">A Random object with the same seed.</param>
        /// <returns>A QuestReward</returns>
        protected abstract QuestReward BuildReward(Random Rng);

        /// <summary>
        /// Proxy method for returning a QuestReward from this design.
        /// </summary>
        /// <param name="Quest">The quest object</param>
        /// <returns>A QuestRewars</returns>
        public QuestReward GetReward(QuestObject Quest)
        {
            return BuildReward(new Random(Quest.Seed + 42));
        }
        
        /// <summary>
        /// The quest tier.
        /// </summary>
        public abstract QuestTier Tier { get; }

        /// <summary>
        /// Called by auxiliary designs in order to get the introductory dialog.
        /// </summary>
        /// <param name="Quest">The quest object</param>
        /// <returns>A keyword to be used in the ThoughtsComponent.</returns>
        public abstract string GetThoughtsKeyword(QuestObject Quest);

        /// <summary>
        /// An unique name for the quest design. Used for loading.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Called by auxiliary designs in order to get the introductory dialog.
        /// </summary>
        /// <param name="Quest">The quest object</param>
        /// <returns>Parameters for the thought keyword.</returns>
        public abstract object[] GetThoughtsParameters(QuestObject Quest);
        
        /// <summary>
        /// Used to show a small description in the plaque.
        /// </summary>
        /// <param name="Quest">The quest object</param>
        /// <returns>A short description of the quest.</returns>
        public abstract string GetShortDescription(QuestObject Quest);

        /// <summary>
        /// It's usually called from the quest journal.
        /// </summary>
        /// <param name="Quest">The quest object</param>
        /// <returns>The quest description</returns>
        public abstract string GetDescription(QuestObject Quest);

        /// <summary>
        /// Used for the quest journal.
        /// </summary>
        /// <param name="Quest">The quest object.</param>
        /// <returns>A QuestView object.</returns>
        public abstract QuestView BuildView(QuestObject Quest);
        
        /// <summary>
        /// Called after building a quest in the Build method.
        /// Used for setting up stuff like components.
        /// </summary>
        /// <param name="Quest">The quest object.</param>
        /// <returns>The same (but edited) quest object.</returns>
        protected virtual QuestObject Setup(QuestObject Quest)
        {
            return Quest;
        }

        /// <summary>
        /// Builds the quest
        /// </summary>
        /// <param name="Previous"></param>
        /// <param name="Context"></param>
        /// <param name="Parameters"></param>
        /// <param name="Rng"></param>
        /// <returns>The same QuestParameters object.</returns>
        protected virtual QuestParameters BuildParameters(QuestObject Previous, QuestContext Context, QuestParameters Parameters, Random Rng)
        {
            return Parameters;
        }

        /// <summary>
        /// Adds a dialog line to the current talk component. Should be used only from QuestDesign::SetupDialog
        /// </summary>
        /// <param name="Quest">The quest object</param>
        /// <param name="Text">The dialog text</param>
        protected void AddDialogLine(QuestObject Quest, Translation Text)
        {
            Quest.Giver.SearchComponent<TalkComponent>().AddDialogLine(Text);
        }

        /// <summary>
        /// List of "bridge" quest designs
        /// </summary>
        protected abstract QuestDesign[] Auxiliaries { get; }

        /// <summary>
        /// The possible quest design descendants for this quest.
        /// </summary>
        protected abstract QuestDesign[] Descendants { get; }

        /// <summary>
        /// Usually called from the QuestInventory
        /// </summary>
        /// <param name="Quest">The quest object</param>
        /// <returns>A bool representing if the quest was completed.</returns>
        public abstract bool IsQuestCompleted(QuestObject Quest);
        
        /// <summary>
        /// Eats up the requirements needed by the quest. e.g. A CollectDesign might consume 3 BERRY.
        /// </summary>
        /// <param name="Quest">The quest object</param>
        protected abstract void Consume(QuestObject Quest);

        /// <summary>
        /// Called by an auxiliary (e.g. SpeakDesign) to setup any dialog needed for the next design.
        /// </summary>
        /// <param name="Quest">The quest object</param>
        /// <param name="Owner">The quest owner</param>
        public virtual void SetupDialog(QuestObject Quest, IPlayer Owner)
        {
            
        }
        
        /// <summary>
        /// Called to cleanup resources when abandoning a quest.
        /// </summary>
        /// <param name="Quest">The quest object</param>
        public virtual void Abandon(QuestObject Quest)
        {
        }
        
        /// <summary>
        /// Called when a quest is completed (but not when a quest is loaded.)
        /// </summary>
        /// <param name="Quest">The quest object</param>
        public void Trigger(QuestObject Quest)
        {
            Consume(Quest);
            Cleanup(Quest);
            if(HasNext)
                Quest.Owner.Questing.Start(GetNext(Quest));
        }

        /// <summary>
        /// Called to cleanup resources generated by the quest. e.g. Quest components.
        /// Usually called either when loading a quest chain or when a quest is completed.
        /// </summary>
        /// <param name="Quest">The quest object.</param>
        public virtual void Cleanup(QuestObject Quest)
        {
            if (Quest.Giver.SearchComponent<QuestComponent>() != null)
                Quest.Giver.RemoveComponent(Quest.Giver.SearchComponent<QuestComponent>());
        }

        /// <summary>
        /// Creates the next quest object of the chain. It could either be an auxiliary or non-auxliary.
        /// </summary>
        /// <param name="Quest">The quest object.</param>
        /// <returns>A new quest object.</returns>
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
                nextObject?.SetSteps(questObject.Steps + 1);
                questObject.Parameters.Set("Next", nextDesign);
                questObject.Parameters.Set("NextObject", nextObject);

                return questObject;
            }
            return Quest.Parameters.Get<QuestObject>("NextObject");
        }

        /// <summary>
        /// Return a bool representing if this is de last non-auxiliary design of the chain.
        /// </summary>
        /// <param name="Quest">The quest object.</param>
        /// <returns>A bool</returns>
        public bool IsEndQuest(QuestObject Quest)
        {
            var rng = new Random(Quest.Parameters.Get<int>("Seed"));
            return Descendants?[rng.Next(0, Descendants.Length)] == null;
        }
        
        /// <summary>
        /// Called for creating new quests from the QuestPool.
        /// </summary>
        /// <param name="Position">The position the quest is given at.</param>
        /// <param name="Rng">For creating the seed.</param>
        /// <param name="Giver">The quest giver.</param>
        /// <returns>A new quest object.</returns>
        public QuestObject Build(Vector3 Position, Random Rng, IHumanoid Giver)
        {
            return Build(new QuestContext(Position), Rng.Next(int.MinValue, int.MaxValue), Giver);
        }

        /// <summary>
        /// Builds a quest object from a current one and a next one.
        /// Used for building the auxiliary quest object in the quest chain.
        /// </summary>
        /// <param name="Object">The current non-auxiliary quest object.</param>
        /// <param name="NextDesign">The next non-auxiliary design</param>
        /// <param name="NextObject">The next non-auxiliary quest object</param>
        /// <returns>A new quest object</returns>
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