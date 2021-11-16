using System.Linq;
using Hedra.Components;
using Hedra.Engine.Localization;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.QuestSystem
{
    public class QuestThoughtsComponent : ThoughtsComponent
    {
        public QuestThoughtsComponent(IEntity Entity, DialogObject Dialog) : base(Entity, Dialog.Arguments)
        {
            BeforeDialog = Dialog.BeforeDialog.Select(Translation.Default).ToArray();
            AfterDialog = Dialog.AfterDialog.Select(Translation.Default).ToArray();
            ThoughtKeyword = Dialog.Keyword;
            UpdateThoughts();
        }

        protected override string ThoughtKeyword { get; }
        public override Translation[] AfterDialog { get; }

        public override Translation[] BeforeDialog { get; }
    }
}