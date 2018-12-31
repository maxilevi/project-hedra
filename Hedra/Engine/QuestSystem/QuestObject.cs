using System;
using System.Text;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.QuestSystem
{
    public class QuestObject
    {
        private readonly QuestDesign _design;
        public string Name => _design.Name;
        public QuestView View => _design.View;
        public QuestTier Tier => _design.Tier;
        public string ShortDescription => _design.GetShortDescription(this);
        public string Description => _design.GetDescription(this);
        public IHumanoid Giver { get; }
        public IPlayer Owner { get; private set; }
        public QuestParameters Parameters { get; }

        public QuestObject(QuestDesign Design, QuestParameters Parameters, IHumanoid Giver)
        {
            _design = Design;
            this.Giver = Giver;
            this.Parameters = Parameters;
        }

        public void Start(IPlayer Player)
        {
            Owner = Player;
        }

        public bool IsQuestCompleted()
        {
            return _design.IsQuestCompleted(this);
        }

        public void Trigger()
        {
            _design.Trigger(this);
        }

        public VertexData BuildPreview()
        {
            return _design.BuildPreview(this);
        }
        
        public byte[] ToArray()
        {
            return Encoding.ASCII.GetBytes(
                QuestTemplate.ToJson(QuestTemplate.FromQuest(this))
            );
        }
        
        public static QuestObject FromArray(byte[] Array)
        {
            var template = QuestTemplate.FromJSON(Encoding.ASCII.GetString(Array));
            return QuestPool.Grab(template.Name).Build(
                new QuestContext(template.Context),
                template.Seed,
                null
            );
        }
    }
}