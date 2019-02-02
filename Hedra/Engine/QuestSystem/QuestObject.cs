using System;
using System.Collections.Generic;
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
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public class QuestObject
    {
        private readonly QuestDesign _design;
        public event OnQuestChanged QuestChanged;
        public QuestView View { get; private set; }
        public QuestDesign BaseDesign { get; }
        public bool IsFirst => Previous == null;
        public bool HasLocation => _design.HasLocation;
        public Vector3 Location => _design.GetLocation(this);
        public QuestReward Reward => _design.GetReward(this);
        public bool IsEndQuest => _design.IsEndQuest(this);
        public string ShortDescription => _design.GetShortDescription(this);
        public string Description => _design.GetDescription(this);
        public int Seed => Parameters.Get<int>("Seed");
        public int Steps { get; private set; }
        public IHumanoid Giver { get; }
        public IPlayer Owner { get; private set; }
        public QuestParameters Parameters { get; }
        public QuestObject Previous { get; set; }
        public bool FirstTime { get; private set; } = true;

        public QuestObject(QuestDesign Design, QuestParameters Parameters, IHumanoid Giver, QuestDesign BaseDesign, int Steps)
        {
            _design = Design;
            this.BaseDesign = BaseDesign;
            this.Steps = Steps;
            this.Giver = Giver;
            this.Parameters = Parameters;
        }

        public void Start(IPlayer Player)
        {
            Owner = Player;
            Owner.Questing.QuestCompleted += Q =>
            {
                if(Q == this)
                    QuestChanged?.Invoke(this);
            };
            Owner.Questing.QuestAbandoned += Q =>
            {
                if(Q == this)
                    QuestChanged?.Invoke(this);
            };
            GenerateContent(Owner);
            View = _design.BuildView(this);
        }

        public void GenerateContent(IPlayer PosibleOwner)
        {
            var previousOwner = Owner;
            try
            {
                Owner = PosibleOwner;
                _design.GenerateContent(this);
            }
            finally
            {
                Owner = previousOwner;
            }
        }

        private void LoadContent(Dictionary<string, object> Content)
        {
            _design.LoadContent(this, Content);
        }

        private QuestObject GetBaseObject()
        {
            if (Previous == null) return this;
            return Previous.GetBaseObject();
        }
        
        private Dictionary<string, object> GetContent()
        {
            return _design.GetContent(this);
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

        public void SetupDialog()
        {
            _design.SetupDialog(this, Owner);
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
                Giver = GiverTemplate.FromHumanoid(Giver),
                Content = GetBaseObject().GetContent()
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
                quest.LoadContent(Template.Content);
                for (var i = 0; i < Template.Steps; ++i)
                {
                    quest._design.Cleanup(quest);
                    quest = quest.Next();
                }

                quest.FirstTime = false;
                QuestPersistence.SetupQuest(quest, quest.Giver, false);
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