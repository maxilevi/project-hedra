using System;
using System.Text;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem.Designs;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.QuestSystem
{
    public class QuestObject
    {
        private readonly QuestDesign _design;
        public QuestView View { get; }
        public QuestDesign BaseDesign { get; }
        public QuestReward Reward => _design.GetReward(this);
        public bool IsEndQuest => _design.IsEndQuest(this);
        public string ShortDescription => _design.GetShortDescription(this);
        public string Description => _design.GetDescription(this);
        public int Seed => Parameters.Get<int>("Seed");
        public int Steps { get; private set; }
        public IHumanoid Giver { get; }
        public IPlayer Owner { get; private set; }
        public QuestParameters Parameters { get; }
        public bool FirstTime { get; private set; } = true;

        public QuestObject(QuestDesign Design, QuestParameters Parameters, IHumanoid Giver, QuestDesign BaseDesign, int Steps)
        {
            _design = Design;
            this.BaseDesign = BaseDesign;
            this.Steps = Steps;
            this.Giver = Giver;
            this.Parameters = Parameters;
            this.View = _design.BuildView(this);
        }

        public void Start(IPlayer Player)
        {
            Owner = Player;
        }

        public void SetSteps(int NewSteps)
        {
            Steps = NewSteps;
        }

        public bool IsQuestCompleted()
        {
            return _design.IsQuestCompleted(this);
        }

        public void Trigger()
        {
            _design.Trigger(this);
        }

        public void Abandon()
        {
            _design.Abandon(this);
        }

        private QuestObject Next()
        {
            return _design.GetNext(this);
        }

        public QuestThoughtsComponent BuildThoughts(IHumanoid Humanoid)
        {
            return new QuestThoughtsComponent(Humanoid, _design.GetThoughtsKeyword(this), _design.GetThoughtsParameters(this));
        }

        public QuestTemplate ToTemplate()
        {
            return new QuestTemplate
            {
                Name = BaseDesign.Name,
                Seed = Parameters.Get<int>("Seed"),
                Context = Parameters.Get<QuestContext>("Context").ContextType,
                Steps = Steps,
                Giver = GiverTemplate.FromHumanoid(Giver)
            };
        }

        public static QuestObject FromTemplate(QuestTemplate Template)
        {
            try
            {
                var quest = QuestPool.Grab(Template.Name).Build(
                    new QuestContext(Template.Context),
                    Template.Seed,
                    QuestPersistence.BuildQuestVillager(Template.Giver)
                );
                for (var i = 0; i < Template.Steps; ++i)
                {
                    quest._design.Cleanup(quest);
                    quest = quest.Next();
                }

                quest.FirstTime = false;
                return quest;
            }
            catch (Exception e)
            {
                Log.WriteLine(e);
                Log.WriteLine($"Failed to load quest {Template.Name}. Ignoring...");
                return null;
            }
        }
    }
}