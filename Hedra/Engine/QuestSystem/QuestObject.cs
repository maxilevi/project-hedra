using System;
using System.Text;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.Templates;
using Hedra.Engine.Player.QuestSystem;

namespace Hedra.Engine.QuestSystem
{
    public class QuestObject
    {
        private readonly QuestDesign _design;
        public string Name => _design.Name;
        public QuestView View => _design.View;
        public QuestTier Tier => _design.Tier;
        public string DisplayName => _design.GetDisplayName(this);
        public string Description => _design.GetDescription(this);
        public QuestParameters Parameters { get; }

        public QuestObject(QuestDesign Design, QuestParameters Parameters)
        {
            _design = Design;
            this.Parameters = Parameters;
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
                template.Seed
            );
        }
        
        public Item ToItem()
        {
            return new Item
            {
                DisplayName = DisplayName,
                Name = DisplayName,
                Description = Description,
                Model = _design.BuildPreview(this)
            };
        }
    }
}