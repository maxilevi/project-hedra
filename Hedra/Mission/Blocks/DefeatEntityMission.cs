using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Mission.Blocks
{
    public class DefeatEntityMission : EntityMission
    {
        public override bool IsCompleted => Entity.IsDead;
        public override string ShortDescription => Translations.Get("quest_defeat_entity_short", Entity.Name);
        public override string Description => Translations.Get("quest_defeat_entity_description", Entity.Name);
        public override DialogObject DefaultOpeningDialog => default;

        public DefeatEntityMission(IEntity Entity) : base(Entity)
        {
        }
    }
}