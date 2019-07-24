using System.Linq;
using Hedra.Components;
using Hedra.Engine.Localization;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.QuestSystem
{
    public class QuestThoughtsComponent : ThoughtsComponent
    {
        private readonly Translation[] _beforeDialog;
        private readonly Translation[] _afterDialog;
        public QuestThoughtsComponent(IEntity Entity, DialogObject Dialog) : base(Entity, Dialog.Arguments)
        {
            _beforeDialog = Dialog.BeforeDialog.Select(Translation.Default).ToArray();
            _afterDialog = Dialog.AfterDialog.Select(Translation.Default).ToArray();
            ThoughtKeyword = Dialog.Keyword;
            UpdateThoughts();
        }

        protected override string ThoughtKeyword { get; }
        public override Translation[] AfterDialog => _afterDialog;
        public override Translation[] BeforeDialog => _beforeDialog;
    }
}